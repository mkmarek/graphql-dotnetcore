namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using Type;

    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument ast;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLOperationDefinition operation;
        private GraphQLSchema graphQLSchema;
        private dynamic variables;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.graphQLSchema = graphQLSchema;
            this.ast = ast;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.variables = new ExpandoObject();
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
            foreach (var definition in this.ast.Definitions)
                this.ResolveDefinition(definition);

            if (this.operation == null)
                throw new Exception("Must provide an operation.");

            return this.ComposeResultForType(this.GetOperationRootType(), this.operation.SelectionSet);
        }

        private dynamic ComposeResultForType(GraphQLObjectType type, GraphQLSelectionSet selectionSet)
        {
            var variableResolver = new VariableResolver(this.variables, this.graphQLSchema.TypeTranslator, this.operation.VariableDefinitions);
            var valueResolver = new ValueResolver(variableResolver, this.graphQLSchema.TypeTranslator);
            var fieldCollector = new FieldCollector(this.fragments, valueResolver);

            var scope = new FieldScope(
                this.graphQLSchema.TypeTranslator,
                valueResolver,
                fieldCollector,
                type,
                null);

            return scope.GetObject(fieldCollector.CollectFields(type, selectionSet));
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

        private void ResolveDefinition(ASTNode definition)
        {
            switch (definition.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    this.ResolveOperationDefinition(definition as GraphQLOperationDefinition); break;
                case ASTNodeKind.FragmentDefinition:
                    this.ResolveFragmentDefinition(definition as GraphQLFragmentDefinition); break;
                default: throw new Exception($"GraphQL cannot execute a request containing a {definition.Kind}.");
            }
        }

        private void ResolveFragmentDefinition(GraphQLFragmentDefinition graphQLFragmentDefinition)
        {
            this.fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (this.operation != null)
                throw new Exception("Must provide operation name if query contains multiple operations.");

            if (this.operation == null)
                this.operation = graphQLOperationDefinition;
        }
    }
}