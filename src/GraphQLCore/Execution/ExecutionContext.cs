namespace GraphQLCore.Execution
{
    using Exceptions;
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

    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument ast;
        private ValidationContext validationContext;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLSchema graphQLSchema;
        private GraphQLOperationDefinition operation;
        private dynamic variables;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.graphQLSchema = graphQLSchema;
            this.ast = ast;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.variables = new ExpandoObject();
            this.validationContext = new ValidationContext();
        }

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast, dynamic variables)
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

            if (this.operation == null && !string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException($"Unknown operation named \"{operationToExecute}\".");
            if (this.operation == null && string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException("Must provide an operation.");

            return this.ComposeResultForType(this.GetOperationRootType(), this.operation.SelectionSet);
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
                new DefaultValuesOfCorrectType(),
                new VariablesInAllowedPositions(),
                new LoneAnonymousOperation(),
                new UniqueInputFieldNames(),
                new UniqueArguments(),
                new UniqueVariableNames(),
                new UniqueOperationNames(),
                new KnownTypeNames(),
                new PossibleFragmentSpreads(),
                new ScalarLeafs(),
                new ArgumentsOfCorrectType(),
                new ProvidedNonNullArguments(),
                new VariablesAreInputTypes()
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

        private dynamic ComposeResultForType(GraphQLObjectType type, GraphQLSelectionSet selectionSet)
        {
            var variableResolver = this.CreateVariableResolver();

            var fieldCollector = new FieldCollector(this.fragments);

            var scope = new FieldScope(
                this.graphQLSchema.SchemaRepository,
                variableResolver,
                fieldCollector,
                type,
                null);

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
                            this.operation?.VariableDefinitions);
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.operation.Operation)
            {
                case OperationType.Query: return this.graphQLSchema.QueryType;
                case OperationType.Mutation: return this.graphQLSchema.MutationType;
                default: throw new Exception($"Can't execute type {this.operation.Operation}");
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

                return scope.CompleteValue(this.graphQLSchema.IntrospectedSchema, field, field.Arguments.ToList());
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

                return scope.CompleteValue(
                    scope.InvokeWithArguments(
                        field.Arguments.ToList(),
                        this.GetTypeIntrospectionLambda()),
                    field,
                    field.Arguments.ToList());
            }

            return null;
        }

        private void ResolveDefinition(ASTNode definition, string operationToExecute)
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
            if (this.operation != null && string.IsNullOrWhiteSpace(operationToExecute))
                throw new GraphQLException("Must provide operation name if query contains multiple operations.");

            if (!string.IsNullOrWhiteSpace(operationToExecute) && graphQLOperationDefinition.Name.Value == operationToExecute)
                this.operation = graphQLOperationDefinition;
            else if (string.IsNullOrWhiteSpace(operationToExecute) && this.operation == null)
                this.operation = graphQLOperationDefinition;
        }
    }
}