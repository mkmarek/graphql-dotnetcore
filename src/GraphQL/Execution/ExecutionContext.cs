namespace GraphQL.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Language.AST;
    using Type;
    using System.Dynamic;
    using System.Linq;
    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument AST;
        private GraphQLSchema GraphQLSchema;
        private GraphQLOperationDefinition Operation;
        private Dictionary<int, object> ResolvedObjectCache;
        private Dictionary<string, GraphQLFragmentDefinition> Fragments;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.GraphQLSchema = graphQLSchema;
            this.AST = ast;
            this.ResolvedObjectCache = new Dictionary<int, object>();
            this.Fragments = new Dictionary<string, GraphQLFragmentDefinition>();
        }

        public void Dispose()
        {
        }

        public dynamic Execute()
        {
            foreach (var definition in this.AST.Definitions)
                this.ResolveDefinition(definition);

            if (this.Operation == null)
                throw new Exception("Must provide an operation.");

            var type = this.GetOperationRootType();
            var fields = this.CollectFields(type, this.Operation.SelectionSet);

            return this.ExecuteFields(type, fields);
        }

        private dynamic ExecuteFields(GraphQLObjectType type, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                foreach (var selection in field.Value)
                    dictionary.Add(field.Key, this.CompleteValue(type.ResolveField(selection, this.ResolvedObjectCache), selection));

            return result;
        }

        private object CompleteValue(object input, GraphQLFieldSelection selection)
        {
            if (input is GraphQLObjectType)
                return this.CompleteObjectType((GraphQLObjectType)input, selection);

            return input;
        }

        private object CompleteObjectType(GraphQLObjectType input, GraphQLFieldSelection selection)
        {
            var fields = this.CollectFields(input, selection.SelectionSet);
            return this.ExecuteFields(input, fields);
        }

        private Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(GraphQLObjectType runtimeType, GraphQLSelectionSet selectionSet)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields);

            return fields;
        }

        private void CollectFieldsInSelection(GraphQLObjectType runtimeType, ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields); break;
                case ASTNodeKind.FragmentSpread: this.CollectFragmentSpreadFields(runtimeType, (GraphQLFragmentSpread)selection, fields); break;
                case ASTNodeKind.InlineFragment: this.CollectFragmentFields(runtimeType, (GraphQLInlineFragment)selection, fields); break;
            }
        }

        private void CollectFragmentSpreadFields(GraphQLObjectType runtimeType, GraphQLFragmentSpread fragmentSpread, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var fragment = this.Fragments[fragmentSpread.Name.Value];
            CollectFragmentFields(runtimeType, fragment, fields);
        }

        private void CollectFragmentFields(GraphQLObjectType runtimeType, GraphQLInlineFragment fragment, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(fragment.Directives))
                return;

            if (!this.DoesFragmentConditionMatch(runtimeType, fragment))
                return;

            this.CollectFields(runtimeType, fragment.SelectionSet)
                .ToList().ForEach(e => fields.Add(e.Key, e.Value));
        }

        private bool DoesFragmentConditionMatch(GraphQLObjectType runtimeType, GraphQLInlineFragment fragment)
        {
            if (fragment.TypeCondition == null)
                return true;

            if (fragment.TypeCondition.Name.Value == runtimeType.Name)
                return true;

            return false;
        }

        private void CollectField(GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(selection.Directives))
                return;

            var entryKey = this.GetFieldEntryKey(selection);

            if (!fields.ContainsKey(entryKey))
                fields.Add(entryKey, new List<GraphQLFieldSelection>());

            fields[entryKey].Add(selection);
        }

        private bool ShouldIncludeNode(IEnumerable<GraphQLDirective> directives)
        {
            var skipAST = directives?.FirstOrDefault(e => e.Name.Value == "skip");
            if (skipAST != null && this.GetArgumentValue(skipAST, "if") == true)
                return false;

            var includeAST = directives?.FirstOrDefault(e => e.Name.Value == "include");
            if (includeAST != null && this.GetArgumentValue(includeAST, "if") == false)
                return false;

            return true;
        }

        private bool GetArgumentValue(GraphQLDirective directive, string argumentName)
        {
            var value = directive.Arguments.SingleOrDefault(e => e.Name.Value == argumentName).Value;

            switch(value.Kind)
            {
                case ASTNodeKind.BooleanValue: return ((GraphQLValue<bool>)value).Value;
            }

            return false;
        }

        private string GetFieldEntryKey(GraphQLFieldSelection selection)
        {
            return selection.Alias != null ? selection.Alias.Value : selection.Name.Value;
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.Operation.Operation)
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
            this.Fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (this.Operation != null)
                throw new Exception("Must provide operation name if query contains multiple operations.");

            if (this.Operation == null)
                this.Operation = graphQLOperationDefinition;
        }
    }
}
