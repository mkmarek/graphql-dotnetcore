namespace GraphQL.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Language.AST;
    using Type;
    using System.Dynamic;
    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument ast;
        private GraphQLSchema graphQLSchema;
        private GraphQLOperationDefinition operation;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.graphQLSchema = graphQLSchema;
            this.ast = ast;
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
            var fields = this.CollectFields(type, this.operation.SelectionSet);

            return this.ExecuteFields(type, fields);
        }

        private dynamic ExecuteFields(GraphQLObjectType type, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                foreach (var selection in field.Value)
                    dictionary.Add(field.Key, type.ResolveField(selection));

            return result;
        }

        private Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(
            GraphQLObjectType type,
            GraphQLSelectionSet selectionSet)
        {
            Dictionary<string, IList<GraphQLFieldSelection>> fields =
                new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(selection, fields);

            return fields;
        }

        private void CollectFieldsInSelection(ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields); break;
            }
        }

        private void CollectField(
            GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var entryKey = this.GetFieldEntryKey(selection);

            if (!fields.ContainsKey(entryKey))
                fields.Add(entryKey, new List<GraphQLFieldSelection>());

            fields[entryKey].Add(selection);
        }

        private string GetFieldEntryKey(GraphQLFieldSelection selection)
        {
            return selection.Alias != null ? selection.Alias.Value : selection.Name.Value;
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.operation.Operation)
            {
                case OperationType.Query: return this.graphQLSchema.RootType;
                default: throw new Exception("Can only execute queries");
            }
        }

        private void ResolveDefinition(ASTNode definition)
        {
            switch (definition.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    this.ResolveOperationDefinition(definition as GraphQLOperationDefinition); break;
                default: throw new Exception($"GraphQL cannot execute a request containing a ${definition.Kind}.");
            }
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
