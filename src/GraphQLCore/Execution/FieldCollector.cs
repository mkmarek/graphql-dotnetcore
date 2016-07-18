namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Scalar;

    public class FieldCollector : IFieldCollector
    {
        private Dictionary<string, GraphQLFragmentDefinition> fragments;

        public FieldCollector(Dictionary<string, GraphQLFragmentDefinition> fragments)
        {
            this.fragments = fragments;
        }

        public Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(GraphQLObjectType runtimeType, GraphQLSelectionSet selectionSet)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields);

            return fields;
        }

        private static GraphQLValue GetIncludeIfArgumentValue(IEnumerable<GraphQLDirective> directives)
        {
            var skipAST = directives?.FirstOrDefault(e => e.Name.Value == "include");

            return skipAST?.Arguments?.SingleOrDefault(e => e.Name.Value == "if")?.Value;
        }

        private static GraphQLValue GetSkipIfArgumentValue(IEnumerable<GraphQLDirective> directives)
        {
            var skipAST = directives?.FirstOrDefault(e => e.Name.Value == "skip");

            return skipAST?.Arguments?.SingleOrDefault(e => e.Name.Value == "if")?.Value;
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
            var boolean = new GraphQLBoolean();
            var shouldSkip = GetSkipIfArgumentValue(directives);
            var shouldContinue = GetIncludeIfArgumentValue(directives);

            if (shouldSkip != null && boolean.GetFromAst(shouldSkip, null).Equals(true))
                return false;

            if (shouldContinue != null && boolean.GetFromAst(shouldContinue, null).Equals(false))
                return false;

            return true;
        }
    }
}