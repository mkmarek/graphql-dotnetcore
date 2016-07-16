namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class FieldCollector : IFieldCollector
    {
        private IValueResolver valueResolver;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;

        public FieldCollector(Dictionary<string, GraphQLFragmentDefinition> fragments, IValueResolver valueResolver)
        {
            this.fragments = fragments;
            this.valueResolver = valueResolver;
        }

        public Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(GraphQLObjectType runtimeType, GraphQLSelectionSet selectionSet)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields);

            return fields;
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

        private void CollectFieldsInSelection(GraphQLObjectType runtimeType, ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields); break;
                case ASTNodeKind.FragmentSpread: this.CollectFragmentSpreadFields(runtimeType, (GraphQLFragmentSpread)selection, fields); break;
                case ASTNodeKind.InlineFragment: this.CollectFragmentFields(runtimeType, (GraphQLInlineFragment)selection, fields); break;
            }
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

        private void CollectFragmentSpreadFields(GraphQLObjectType runtimeType, GraphQLFragmentSpread fragmentSpread, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var fragment = this.fragments[fragmentSpread.Name.Value];
            this.CollectFragmentFields(runtimeType, fragment, fields);
        }

        private bool DoesFragmentConditionMatch(GraphQLObjectType runtimeType, GraphQLInlineFragment fragment)
        {
            if (fragment.TypeCondition == null)
                return true;

            if (fragment.TypeCondition.Name.Value == runtimeType.Name)
                return true;

            return false;
        }

        private string GetFieldEntryKey(GraphQLFieldSelection selection)
        {
            return selection.Alias != null ? selection.Alias.Value : selection.Name.Value;
        }

        private bool ShouldIncludeNode(IEnumerable<GraphQLDirective> directives)
        {
            var skipAST = directives?.FirstOrDefault(e => e.Name.Value == "skip");
            if (skipAST != null && this.valueResolver.GetArgumentValue(skipAST.Arguments, "if").Equals(true))
                return false;

            var includeAST = directives?.FirstOrDefault(e => e.Name.Value == "include");
            if (includeAST != null && this.valueResolver.GetArgumentValue(includeAST.Arguments, "if").Equals(false))
                return false;

            return true;
        }
    }
}