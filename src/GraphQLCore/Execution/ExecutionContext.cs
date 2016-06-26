namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using Type;

    public class ExecutionContext : IDisposable
    {
        internal GraphQLSchema GraphQLSchema;
        private GraphQLDocument ast;
        private FieldCollector fieldCollector;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLOperationDefinition operation;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.GraphQLSchema = graphQLSchema;
            this.ast = ast;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.fieldCollector = new FieldCollector(this.fragments);
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

            var type = this.GetOperationRootType();

            return ComposeResultForType(type, this.operation.SelectionSet);
        }

        internal dynamic ComposeResultForType(GraphQLObjectType type, GraphQLSelectionSet selectionSet, object parentObject = null)
        {
            var scope = CreateScope(type, parentObject);
            return GetResultFromScope(type, selectionSet, scope);
        }

        internal FieldScope CreateScope(GraphQLObjectType type, object parentObject)
        {
            return new FieldScope(this, type, parentObject);
        }

        internal dynamic GetResultFromScope(GraphQLObjectType type, GraphQLSelectionSet selectionSet, FieldScope scope)
        {
            var fields = this.fieldCollector.CollectFields(type, selectionSet);
            return scope.GetObject(fields);
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.operation.Operation)
            {
                case OperationType.Query: return this.GraphQLSchema.RootType;
                default: throw new Exception("Can only execute queries");
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