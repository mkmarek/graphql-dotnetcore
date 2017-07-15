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
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLSchema graphQLSchema;
        private dynamic variables;
        private int? subscriptionId;
        private string clientId;

        public GraphQLOperationDefinition Operation { get; private set; }

        private ExecutionManager(GraphQLSchema graphQLSchema, dynamic variables, string clientId, int? subscriptionId)
        {
            this.graphQLSchema = graphQLSchema;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.validationContext = new ValidationContext();

            this.variables = variables ?? new ExpandoObject();
            this.subscriptionId = subscriptionId;
            this.clientId = clientId;
        }

        public ExecutionManager(GraphQLSchema graphQLSchema, GraphQLDocument ast, object variables = null, string clientId = null, int? subscriptionId = null)
            : this(graphQLSchema, variables, clientId, subscriptionId)
        {
            this.ast = ast;
        }

        public ExecutionManager(GraphQLSchema graphQLSchema, string expression, object variables = null, string clientId = null, int? subscriptionId = null)
            : this(graphQLSchema, variables, clientId, subscriptionId)
        {
            this.expression = expression;
        }

        public void Dispose()
        {
        }

        public async Task<dynamic> ExecuteAsync()
        {
            return await this.ExecuteAsync(null);
        }

        private async Task<dynamic> ExecuteAsyncWithErrors(string operationToExecute)
        {
            if (this.ast == null)
                this.ast = GraphQLDocument.GetAst(this.expression);

            foreach (var definition in this.ast.Definitions)
                this.ResolveDefinition(definition, operationToExecute);

            this.CreateVariableResolver();

            this.ValidateAstAndThrowErrorWhenFaulty();

            if (this.Operation == null && !string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException($"Unknown operation named \"{operationToExecute}\".");
            if (this.Operation == null && string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException("Must provide an operation.");

            var operationType = this.GetOperationRootType();

            if (this.Operation.Operation == OperationType.Subscription)
                return await this.ComposeResultForSubscriptions(operationType, this.Operation);
            else if (this.Operation.Operation == OperationType.Query)
                return await this.ComposeResultForQuery(operationType, this.Operation);
            else //Mutation
                return await this.ComposeResultForMutation(operationType, this.Operation);
        }

        public async Task<dynamic> ExecuteAsync(string operationToExecute)
        {
            try
            {
                return await this.ExecuteAsyncWithErrors(operationToExecute);
            }
            catch (GraphQLException ex)
            {
                return this.CreateResultObjectForErrors(new[] { ex });
            }
            catch (GraphQLValidationException ex)
            {
                return this.CreateResultObjectForErrors(ex.Errors);
            }
            catch (Exception ex)
            {
                return this.CreateResultObjectForErrors(new[] { new GraphQLException(ex) });
            }
        }

        private void ValidateAstAndThrowErrorWhenFaulty()
        {
            var errors = this.validationContext.Validate(this.ast, this.graphQLSchema, this.GetValidationRules());

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

        private async Task AppendIntrospectionInfo(
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

        public async Task<ExpandoObject> ComposeResultForSubscriptions(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition)
        {
            if (string.IsNullOrWhiteSpace(this.clientId))
            {
                throw new GraphQLException(
                    "Can't invoke subscription without clientId specified",
                    new ASTNode[] { operationDefinition });
            }

            if (!this.subscriptionId.HasValue)
            {
                throw new GraphQLException(
                    "Can't invoke subscription without subscriptionId specified",
                    new ASTNode[] { operationDefinition });
            }

            var context = this.CreateExecutionContext(operationDefinition);

            var scope = new FieldScope(context, type, null);

            return await this.ProcessSubscription(
                    (GraphQLSubscriptionType)type,
                    context.FieldCollector,
                    scope);
        }

        public async Task<ExpandoObject> ComposeResultForQuery(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition, object parent = null)
        {
            var context = this.CreateExecutionContext(operationDefinition);
            var scope = new FieldScope(context, type, parent);

            var fields = context.FieldCollector.CollectFields(type, operationDefinition.SelectionSet);
            var resultObject = await scope.GetObject(fields);

            await this.AppendIntrospectionInfo(scope, fields, resultObject);

            var returnObject = new ExpandoObject();
            var returnObjectDictionary = (IDictionary<string, object>)returnObject;

            returnObjectDictionary.Add("data", resultObject);

            if (scope.Errors.Any())
                returnObjectDictionary.Add("errors", scope.Errors);

            return returnObject;
        }

        public async Task<ExpandoObject> ComposeResultForMutation(
            GraphQLComplexType type, GraphQLOperationDefinition operationDefinition)
        {
            var context = this.CreateExecutionContext(operationDefinition);
            var scope = new FieldScope(context, type, null);

            var fields = context.FieldCollector.CollectFields(type, operationDefinition.SelectionSet);
            var resultObject = await scope.GetObjectSynchronously(fields);

            await this.AppendIntrospectionInfo(scope, fields, resultObject);

            var returnObject = new ExpandoObject();
            var returnObjectDictionary = (IDictionary<string, object>)returnObject;

            returnObjectDictionary.Add("data", resultObject);

            if (scope.Errors.Any())
                returnObjectDictionary.Add("errors", scope.Errors);

            return returnObject;
        }

        private ExecutionContext CreateExecutionContext(GraphQLOperationDefinition operationDefinition)
        {
            var variableResolver = this.CreateVariableResolver();

            var fieldCollector = new FieldCollector(
                this.fragments,
                this.graphQLSchema.SchemaRepository);

            return new ExecutionContext()
            {
                FieldCollector = fieldCollector,
                OperationType = operationDefinition.Operation,
                Schema = this.graphQLSchema,
                SchemaRepository = this.graphQLSchema.SchemaRepository,
                VariableResolver = variableResolver
            };
        }

        private async Task<ExpandoObject> ProcessSubscription(
            GraphQLSubscriptionType type,
            IFieldCollector fieldCollector,
            FieldScope scope)
        {
            var fields = fieldCollector.CollectFields(type, this.Operation.SelectionSet);
            var field = fields.Single(); //only single subscription field allowed
            var result = new ExpandoObject();
            var resultDictionary = (IDictionary<string, object>)result;

            await this.RegisterSubscription(
                field.Value.Single(),
                type,
                this.ast,
                scope);

            resultDictionary.Add("subscriptionId", this.subscriptionId.Value);
            resultDictionary.Add("clientId", this.clientId);

            var returnObject = new ExpandoObject();
            var returnObjectDictionary = (IDictionary<string, object>)returnObject;

            returnObjectDictionary.Add("data", result);

            if (scope.Errors.Any())
                returnObjectDictionary.Add("errors", scope.Errors);

            return returnObject;
        }

        private ExpandoObject CreateResultObjectForErrors(IEnumerable<GraphQLException> errors)
        {
            var resultObject = new ExpandoObject();
            var resultObjectDictionary = (IDictionary<string, object>)resultObject;

            resultObjectDictionary.Add("errors", errors);

            return resultObject;
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
                this.subscriptionId.Value,
                this.Operation?.Name?.Value ?? "Anonymous",
                this.variables,
                filter,
                this.ast));
        }

        private VariableResolver CreateVariableResolver()
        {
            return new VariableResolver(
                            this.variables,
                            this.graphQLSchema.SchemaRepository,
                            this.Operation?.VariableDefinitions);
        }

        private GraphQLComplexType GetOperationRootType()
        {
            switch (this.Operation.Operation)
            {
                case OperationType.Query:
                    return this.graphQLSchema.QueryType;

                case OperationType.Mutation:
                    if (this.graphQLSchema.MutationType == null)
                        throw new GraphQLException("Schema is not configured for mutations",
                            new[] { this.Operation });
                    return this.graphQLSchema.MutationType;

                case OperationType.Subscription:
                    if (this.graphQLSchema.SubscriptionType == null)
                        throw new GraphQLException("Schema is not configured for subscriptions",
                            new[] { this.Operation });
                    return this.graphQLSchema.SubscriptionType;

                default:
                    throw new GraphQLException("Can only execute queries, mutations and subscriptions",
                        new[] { this.Operation });
            }
        }

        private Expression<Func<string, IntrospectedType>> GetTypeIntrospectionLambda()
        {
            return (string name) => this.graphQLSchema.IntrospectType(name);
        }

        private async Task<object> IntrospectSchemaIfRequested(
            FieldScope scope, IDictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (fields.ContainsKey("__schema"))
            {
                var field = fields["__schema"].Single();
                fields.Remove("__schema");

                return await scope.CompleteValue(
                    this.graphQLSchema.IntrospectedSchema,
                    this.graphQLSchema.IntrospectedSchema.GetType(),
                    field,
                    field.Arguments.ToList());
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

                return await scope.CompleteValue(
                    value,
                    value?.GetType(),
                    field,
                    field.Arguments.ToList());
            }

            return null;
        }

        public void ResolveDefinition(ASTNode definition, string operationToExecute)
        {
            switch (definition.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    this.ResolveOperationDefinition(definition as GraphQLOperationDefinition, operationToExecute); break;
                case ASTNodeKind.FragmentDefinition:
                    this.ResolveFragmentDefinition(definition as GraphQLFragmentDefinition); break;
                default: throw new GraphQLException($"GraphQL cannot execute a request containing a {definition.Kind}.",
                    new[] { definition });
            }
        }

        private void ResolveFragmentDefinition(GraphQLFragmentDefinition graphQLFragmentDefinition)
        {
            if (!this.fragments.ContainsKey(graphQLFragmentDefinition.Name.Value))
                this.fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition, string operationToExecute)
        {
            if (this.Operation != null && string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException("Must provide operation name if query contains multiple operations.");

            if (!string.IsNullOrWhiteSpace(operationToExecute) && graphQLOperationDefinition.Name.Value == operationToExecute)
                this.Operation = graphQLOperationDefinition;
            else if (string.IsNullOrWhiteSpace(operationToExecute) && this.Operation == null)
                this.Operation = graphQLOperationDefinition;
        }
    }
}