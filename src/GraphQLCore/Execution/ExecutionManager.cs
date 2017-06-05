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
    using Type;
    using Type.Introspection;
    using Validation;
    using Validation.Rules;

    public class ExecutionManager : IDisposable
    {
        private GraphQLDocument ast;
        private ValidationContext validationContext;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLSchema graphQLSchema;
        private dynamic variables;

        public GraphQLOperationDefinition Operation { get; private set; }

        public ExecutionManager(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.graphQLSchema = graphQLSchema;
            this.ast = ast;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.variables = new ExpandoObject();
            this.validationContext = new ValidationContext();
        }

        public ExecutionManager(GraphQLSchema graphQLSchema, GraphQLDocument ast, dynamic variables)
            : this(graphQLSchema, ast)
        {
            this.variables = variables;
        }

        public void Dispose()
        {
        }

        public dynamic Execute()
        {
            return this.Execute(null);
        }

        public dynamic Execute(string operationToExecute)
        {
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
                return this.ComposeResultForSubscriptions(operationType, this.Operation);
            else
                return this.ComposeResultForQueryAndMutation(operationType, this.Operation);
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
                new OverlappingFieldsCanBeMerged()
            };
        }

        private void AppendIntrospectionInfo(
            FieldScope scope, Dictionary<string, IList<GraphQLFieldSelection>> fields, dynamic resultObject)
        {
            var introspectedSchema = this.IntrospectSchemaIfRequested(scope, fields);
            var introspectedField = this.IntrospectTypeIfRequested(scope, fields);

            if (introspectedSchema != null)
                resultObject.__schema = introspectedSchema;

            if (introspectedField != null)
                resultObject.__type = introspectedField;
        }

        public dynamic ComposeResultForSubscriptions(GraphQLComplexType type, GraphQLOperationDefinition operationDefinition)
        {
            var context = this.CreateExecutionContext(operationDefinition);

            var scope = new FieldScope(context, type, null);

            return this.ProcessSubscriptions(
                    (GraphQLSubscriptionType)type,
                    context.FieldCollector,
                    scope);
        }

        public dynamic ComposeResultForQueryAndMutation(GraphQLComplexType type, GraphQLOperationDefinition operationDefinition)
        {
            var context = this.CreateExecutionContext(operationDefinition);
            var scope = new FieldScope(context, type, null);

            return this.ResolveQueryAndMutationResult(
                    type,
                    operationDefinition.SelectionSet,
                    context.FieldCollector,
                    scope);
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

        private ExpandoObject ProcessSubscriptions(
            GraphQLSubscriptionType type,
            IFieldCollector fieldCollector,
            FieldScope scope)
        {
            var fields = fieldCollector.CollectFields(type, this.Operation.SelectionSet);
            var result = new ExpandoObject();
            var resultDictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
            {
                var subscriptionId = this.RegisterSubscription(
                        field.Value.Single(),
                        type,
                        this.ast,
                        scope);

                resultDictionary.Add(field.Key, subscriptionId);
            }

            return result;
        }

        private long RegisterSubscription(
            GraphQLFieldSelection fieldSelection,
            GraphQLSubscriptionType type,
            GraphQLDocument document,
            FieldScope scope)
        {
            var fieldInfo = type.GetFieldInfo(fieldSelection.Name.Value) as GraphQLSubscriptionTypeFieldInfo;

            Expression<Func<object, bool>> filter
                = entity => (bool)scope.InvokeWithArguments(fieldSelection.Arguments.ToList(), fieldInfo.Filter, entity);

            type.EventBus.Subscribe(EventBusSubscription.Create(
                fieldInfo.Channel,
                Guid.NewGuid().ToString(),
                this.Operation.Name.Value,
                this.variables,
                filter,
                this.ast));

            return 5456;
        }

        private dynamic ResolveQueryAndMutationResult(
            GraphQLComplexType type,
            GraphQLSelectionSet selectionSet,
            IFieldCollector fieldCollector,
            FieldScope scope)
        {
            var fields = fieldCollector.CollectFields(type, selectionSet);
            var resultObject = scope.GetObject(fields);

            this.AppendIntrospectionInfo(scope, fields, resultObject);

            return resultObject;
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
                case OperationType.Query: return this.graphQLSchema.QueryType;
                case OperationType.Mutation: return this.graphQLSchema.MutationType;
                case OperationType.Subscription: return this.graphQLSchema.SubscriptionType;
                default: throw new Exception($"Can't execute type {this.Operation.Operation}");
            }
        }

        private Expression<Func<string, IntrospectedType>> GetTypeIntrospectionLambda()
        {
            return (string name) => this.graphQLSchema.IntrospectType(name);
        }

        private object IntrospectSchemaIfRequested(
            FieldScope scope, IDictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (fields.ContainsKey("__schema"))
            {
                var field = fields["__schema"].Single();
                fields.Remove("__schema");

                return scope.CompleteValue(
                    this.graphQLSchema.IntrospectedSchema,
                    this.graphQLSchema.IntrospectedSchema.GetType(),
                    field,
                    field.Arguments.ToList());
            }

            return null;
        }

        private object IntrospectTypeIfRequested(
            FieldScope scope, IDictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (fields.ContainsKey("__type"))
            {
                var field = fields["__type"].Single();
                fields.Remove("__type");

                var value = scope.InvokeWithArguments(
                        field.Arguments.ToList(),
                        this.GetTypeIntrospectionLambda());

                return scope.CompleteValue(
                    value,
                    value.GetType(),
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
                default: throw new Exception($"GraphQL cannot execute a request containing a {definition.Kind}.");
            }
        }

        private void ResolveFragmentDefinition(GraphQLFragmentDefinition graphQLFragmentDefinition)
        {
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