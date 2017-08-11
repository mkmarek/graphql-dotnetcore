namespace GraphQLCore.Execution
{
    using Exceptions;
    using GraphQLCore.Events;
    using GraphQLCore.Type.Complex;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Type;
    using Type.Introspection;
    using Validation;
    using Validation.Rules;

    public class ExecutionManager : IDisposable
    {
        private string expression;
        private GraphQLDocument ast;
        private ValidationContext validationContext;
        protected Dictionary<string, GraphQLFragmentDefinition> Fragments { get; }
        protected GraphQLSchema GraphQLSchema { get; }
        private dynamic variables;
        private string subscriptionId;
        private string clientId;
        public Queue<FieldExecution> PostponedExecutions { get; set; }
        private IFieldCollector fieldCollector;

        public GraphQLOperationDefinition Operation { get; private set; }

        public ExecutionManager(GraphQLSchema graphQLSchema, GraphQLDocument ast, object variables = null, string clientId = null, string subscriptionId = null)
            : this(graphQLSchema, variables, clientId, subscriptionId)
        {
            this.ast = ast;
        }

        public ExecutionManager(GraphQLSchema graphQLSchema, string expression, object variables = null, string clientId = null, string subscriptionId = null)
            : this(graphQLSchema, variables, clientId, subscriptionId)
        {
            this.expression = expression;
        }

        private ExecutionManager(GraphQLSchema graphQLSchema, dynamic variables, string clientId, string subscriptionId)
        {
            this.GraphQLSchema = graphQLSchema;
            this.Fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.validationContext = new ValidationContext();

            this.variables = variables ?? new ExpandoObject();
            this.subscriptionId = subscriptionId;
            this.clientId = clientId;

            this.PostponedExecutions = new Queue<FieldExecution>();
        }

        public void Dispose()
        {
        }

        public async Task<ExecutionResult> ExecuteAsync()
        {
            return await this.ExecuteAsync(null);
        }

        public async Task<ExecutionResult> ExecuteAsync(string operationToExecute)
        {
            try
            {
                return await this.ExecuteAsyncWithErrors(operationToExecute);
            }
            catch (GraphQLException ex)
            {
                return this.CreateResultObjectForErrors(ex);
            }
            catch (GraphQLValidationException ex)
            {
                return this.CreateResultObjectForErrors(ex.Errors.ToArray());
            }
            catch (Exception ex)
            {
                return this.CreateResultObjectForErrors(new GraphQLException(ex));
            }
        }

        public IObservable<ExecutionResult> Subscribe(string operationToExecute)
        {
            this.GetAstAndResolveDefinitions(operationToExecute);
            var executionResults = this.Execute(operationToExecute).Select(e => e.ToObservable());

            if (this.Operation.Operation == OperationType.Subscription)
            {
                var subscriptionResult = executionResults.First();
                var result = subscriptionResult.GetAwaiter().GetResult();

                if (result.Errors.Any())
                    return subscriptionResult;

                var subscriptionResults = Observable.FromEventPattern<EventHandler<OnMessageReceivedEventArgs>, OnMessageReceivedEventArgs>(
                        handler => this.GraphQLSchema.OnSubscriptionMessageReceived += handler,
                        handler => this.GraphQLSchema.OnSubscriptionMessageReceived -= handler)
                    .Where(e => e.EventArgs.ClientId == this.clientId)
                    .Select(e => FromEventArgs(e.EventArgs));

                return subscriptionResults;
            }

            return executionResults.Merge();
        }

        private static ExecutionResult FromEventArgs(OnMessageReceivedEventArgs args)
        {
            var result = args.Data as ExecutionResult;

            return result;
        }

        public async Task<SubscriptionExecutionResult> ComposeResultForSubscriptions(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition)
        {
            this.CheckSubscriptionIds(operationDefinition);

            var context = this.CreateExecutionContext(operationDefinition);
            var scope = new FieldScope(context, type);

            return await this.ProcessSubscription(
                (GraphQLSubscriptionType)type,
                context.FieldCollector,
                scope);
        }

        public virtual async Task<ExecutionResult> ComposeResult(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition, object parent = null)
        {
            var context = this.CreateExecutionContext(operationDefinition);
            this.fieldCollector = context.FieldCollector;
            var scope = new FieldScope(context, type, null, parent);

            var fields = context.FieldCollector.CollectFields(type, operationDefinition.SelectionSet, scope);
            var resultObject = operationDefinition.Operation == OperationType.Mutation
                ? await scope.GetObjectSynchronously(fields)
                : await scope.GetObject(fields);

            await this.AppendIntrospectionInfo(scope, fields, resultObject);

            return new ExecutionResult()
            {
                Data = resultObject,
                Errors = scope.Errors.Any() ? scope.Errors : null
            };
        }

        public void ResolveDefinition(ASTNode definition, string operationToExecute)
        {
            switch (definition.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    this.ResolveOperationDefinition(definition as GraphQLOperationDefinition, operationToExecute); break;
                case ASTNodeKind.FragmentDefinition:
                    this.ResolveFragmentDefinition(definition as GraphQLFragmentDefinition); break;
                default:
                    throw new GraphQLException($"GraphQL cannot execute a request containing a {definition.Kind}.",
                        new[] { definition });
            }
        }

        protected async Task AppendIntrospectionInfo(
            FieldScope scope, Dictionary<string, IList<GraphQLFieldSelection>> fields, IDictionary<string, object> resultObject)
        {
            var introspectedSchema = await this.IntrospectSchemaIfRequested(scope, fields);
            var introspectedField = await this.IntrospectTypeIfRequested(scope, fields);

            if (introspectedSchema != null)
            {
                resultObject.Remove("__schema");
                resultObject.Add("__schema", introspectedSchema);
            }

            if (introspectedField != null)
            {
                resultObject.Remove("__type");
                resultObject.Add("__type", introspectedField);
            }
        }

        protected virtual ExecutionContext CreateExecutionContext(GraphQLOperationDefinition operationDefinition)
        {
            var variableResolver = this.CreateVariableResolver();

            var fieldCollector = new FieldCollector(
                this.Fragments,
                this.GraphQLSchema.SchemaRepository);

            var argumentFetcher = new ArgumentFetcher(this.GraphQLSchema.SchemaRepository);

            var context = new ExecutionContext()
            {
                FieldCollector = fieldCollector,
                OperationType = operationDefinition.Operation,
                Schema = this.GraphQLSchema,
                SchemaRepository = this.GraphQLSchema.SchemaRepository,
                VariableResolver = variableResolver,
                ArgumentFetcher = argumentFetcher
            };

            var valueCompleter = new ValueCompleter(context);
            context.ValueCompleter = valueCompleter;

            return context;
        }

        protected VariableResolver CreateVariableResolver()
        {
            return new VariableResolver(
                this.variables,
                this.GraphQLSchema.SchemaRepository,
                this.Operation?.VariableDefinitions);
        }

        private void ResolveDefinitions(string operationToExecute)
        {
            foreach (var definition in this.ast.Definitions)
                this.ResolveDefinition(definition, operationToExecute);
        }

        private void GetAstAndResolveDefinitions(string operationToExecute)
        {
            if (this.ast == null)
                this.ast = GraphQLDocument.GetAst(this.expression);

            if (this.Operation == null)
                this.ResolveDefinitions(operationToExecute);
        }

        private async Task<ExecutionResult> ExecuteAsyncWithErrors(string operationToExecute)
        {
            this.GetAstAndResolveDefinitions(operationToExecute);
            this.CreateVariableResolver();
            this.ValidateAstAndThrowErrorWhenFaulty();

            if (this.Operation == null && !string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException($"Unknown operation named \"{operationToExecute}\".");
            if (this.Operation == null && string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException("Must provide an operation.");

            var operationType = this.GetOperationRootType();

            if (this.Operation.Operation == OperationType.Subscription)
                return await this.ComposeResultForSubscriptions(operationType, this.Operation);

            return await this.ComposeResult(operationType, this.Operation);
        }

        private void CheckSubscriptionIds(GraphQLOperationDefinition operationDefinition)
        {
            if (string.IsNullOrWhiteSpace(this.clientId))
            {
                throw new GraphQLException(
                    "Can't invoke subscription without clientId specified",
                    new ASTNode[] { operationDefinition });
            }

            if (this.subscriptionId == null)
            {
                throw new GraphQLException(
                    "Can't invoke subscription without subscriptionId specified",
                    new ASTNode[] { operationDefinition });
            }
        }

        private IEnumerable<Task<ExecutionResult>> Execute(string operationToExecute)
        {
            var firstTask = this.ExecuteAsync(operationToExecute);
            Task.WaitAll(firstTask);
            yield return firstTask;

            var queue = this.fieldCollector?.PostponedFieldQueue;
            var isRunning = queue?.Count > 0;
            var notCompletedTasks = new List<Task>();

            while (isRunning)
            {
                if (queue.Any())
                {
                    var execution = queue.Dequeue();
                    var task = execution.GetResult();
                    notCompletedTasks.Add(task);

                    yield return task;
                }

                Task.WaitAll(notCompletedTasks.ToArray());
                notCompletedTasks = new List<Task>();
                if (!queue.Any())
                    isRunning = false;
            }
        }

        private void ValidateAstAndThrowErrorWhenFaulty()
        {
            var errors = this.validationContext.Validate(this.ast, this.GraphQLSchema, this.GetValidationRules());

            if (errors.Any())
            {
                throw new GraphQLValidationException(
                    "One or more validation errors were found. See the Errors property for more information",
                    errors);
            }
        }

        private IValidationRule[] GetValidationRules()
        {
            return new IValidationRule[]
            {
                new NoUnusedVariables(),
                new NoUndefinedVariables(),
                new DefaultValuesOfCorrectType(),
                new VariablesInAllowedPositions(),
                new LoneAnonymousOperation(),
                new UniqueInputFieldNames(),
                new UniqueArguments(),
                new UniqueVariableNames(),
                new UniqueOperationNames(),
                new UniqueFragmentNames(),
                new KnownTypeNames(),
                new PossibleFragmentSpreads(),
                new ScalarLeafs(),
                new ArgumentsOfCorrectType(),
                new ProvidedNonNullArguments(),
                new VariablesAreInputTypes(),
                new NoUnusedFragments(),
                new NoFragmentCycles(),
                new KnownFragmentNames(),
                new KnownArgumentNames(),
                new FieldsOnCorrectType(),
                new KnownDirectives(),
                new FragmentsOnCompositeTypes(),
                new OverlappingFieldsCanBeMerged(),
                new UniqueDirectivesPerLocation(),
                new SingleFieldSubscriptions(),
            };
        }

        private async Task<SubscriptionExecutionResult> ProcessSubscription(
            GraphQLSubscriptionType type,
            IFieldCollector fieldCollector,
            FieldScope scope)
        {
            var fields = fieldCollector.CollectFields(type, this.Operation.SelectionSet, scope);
            var field = fields.Single(); // only single subscription field allowed

            await this.RegisterSubscription(
                field.Value.Single(),
                type,
                this.ast,
                scope);

            return new SubscriptionExecutionResult()
            {
                Errors = scope.Errors,
                SubscriptionId = this.subscriptionId
            };
        }

        private ExecutionResult CreateResultObjectForErrors(params GraphQLException[] errors)
        {
            return new ExecutionResult()
            {
                Errors = errors
            };
        }

        private async Task RegisterSubscription(
            GraphQLFieldSelection fieldSelection,
            GraphQLSubscriptionType type,
            GraphQLDocument document,
            FieldScope scope)
        {
            var fieldInfo = type.GetFieldInfo(fieldSelection.Name.Value) as GraphQLSubscriptionTypeFieldInfo;

            Expression<Func<object, bool>> filter = null;

            if (fieldInfo.Filter != null)
            {
                filter = entity => (bool)scope.InvokeWithArgumentsSync(
                    fieldSelection.Arguments.ToList(), fieldInfo.Filter, entity);
            }

            await type.EventBus.Subscribe(EventBusSubscription.Create(
                fieldInfo.Channel,
                this.clientId,
                this.subscriptionId,
                this.Operation?.Name?.Value ?? "Anonymous",
                this.variables,
                filter,
                this.ast));
        }

        private GraphQLComplexType GetOperationRootType()
        {
            switch (this.Operation.Operation)
            {
                case OperationType.Query:
                    return this.GraphQLSchema.QueryType;

                case OperationType.Mutation:
                    if (this.GraphQLSchema.MutationType == null)
                        throw new GraphQLException("Schema is not configured for mutations",
                            new[] { this.Operation });
                    return this.GraphQLSchema.MutationType;

                case OperationType.Subscription:
                    if (this.GraphQLSchema.SubscriptionType == null)
                        throw new GraphQLException("Schema is not configured for subscriptions",
                            new[] { this.Operation });
                    return this.GraphQLSchema.SubscriptionType;

                default:
                    throw new GraphQLException("Can only execute queries, mutations and subscriptions",
                        new[] { this.Operation });
            }
        }

        private Expression<Func<string, IntrospectedType>> GetTypeIntrospectionLambda()
        {
            return (string name) => this.GraphQLSchema.IntrospectType(name);
        }

        private async Task<object> IntrospectSchemaIfRequested(
            FieldScope scope, IDictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (fields.ContainsKey("__schema"))
            {
                var field = fields["__schema"].Single();
                fields.Remove("__schema");

                var executedField = new ExecutedField()
                {
                    Selection = field,
                    Path = new List<object>() { "__schema" }
                };

                return await scope.Context.ValueCompleter.CompleteValue(executedField, this.GraphQLSchema.IntrospectedSchema);
            }

            return null;
        }

        private async Task<object> IntrospectTypeIfRequested(
            FieldScope scope, IDictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (fields.ContainsKey("__type"))
            {
                var field = fields["__type"].Single();
                fields.Remove("__type");

                var value = await scope.InvokeWithArguments(
                        field.Arguments.ToList(),
                        this.GetTypeIntrospectionLambda());

                var executedField = new ExecutedField()
                {
                    Selection = field,
                    Path = new List<object>() { "__type" }
                };

                return await scope.Context.ValueCompleter.CompleteValue(executedField, value);
            }

            return null;
        }

        private void ResolveFragmentDefinition(GraphQLFragmentDefinition graphQLFragmentDefinition)
        {
            if (!this.Fragments.ContainsKey(graphQLFragmentDefinition.Name.Value))
                this.Fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition, string operationToExecute)
        {
            if (this.Operation != null && string.IsNullOrWhiteSpace(operationToExecute) && this.Operation.Name.Value != operationToExecute)
                throw new GraphQLException("Must provide operation name if query contains multiple operations.");

            if (!string.IsNullOrWhiteSpace(operationToExecute) && graphQLOperationDefinition.Name.Value == operationToExecute)
                this.Operation = graphQLOperationDefinition;
            else if (string.IsNullOrWhiteSpace(operationToExecute) && this.Operation == null)
                this.Operation = graphQLOperationDefinition;
        }
    }
}