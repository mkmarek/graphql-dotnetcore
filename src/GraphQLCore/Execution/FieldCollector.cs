namespace GraphQLCore.Execution
{
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class FieldCollector : IFieldCollector
    {
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private ISchemaRepository schemaRepository;

        public FieldCollector(
            Dictionary<string, GraphQLFragmentDefinition> fragments,
            ISchemaRepository schemaRepository)
        {
            this.fragments = fragments;
            this.schemaRepository = schemaRepository;
        }

        public Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(
            GraphQLComplexType runtimeType, GraphQLSelectionSet selectionSet)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields);

            return fields;
        }

        private void CollectField(GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(selection.Directives, DirectiveLocation.FIELD))
                return;

            var entryKey = this.GetFieldEntryKey(selection);

            if (!fields.ContainsKey(entryKey))
                fields.Add(entryKey, new List<GraphQLFieldSelection>());

            fields[entryKey].Add(selection);
        }

        private void CollectFieldsInSelection(GraphQLComplexType runtimeType, ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields); break;
                case ASTNodeKind.FragmentSpread: this.CollectFragmentSpreadFields(runtimeType, (GraphQLFragmentSpread)selection, fields); break;
                case ASTNodeKind.InlineFragment: this.CollectFragmentFields(runtimeType, (GraphQLInlineFragment)selection, fields); break;
            }
        }

        private void CollectFragmentFields(GraphQLComplexType runtimeType, GraphQLInlineFragment fragment, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(fragment.Directives, DirectiveLocation.INLINE_FRAGMENT))
                return;

            if (!this.DoesFragmentConditionMatch(runtimeType, fragment))
                return;

            this.CollectFields(runtimeType, fragment.SelectionSet)
                .ToList().ForEach(e => fields.Add(e.Key, e.Value));
        }

        private void CollectFragmentSpreadFields(GraphQLComplexType runtimeType, GraphQLFragmentSpread fragmentSpread, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(fragmentSpread.Directives, DirectiveLocation.FRAGMENT_SPREAD))
                return;

            var fragment = this.fragments[fragmentSpread.Name.Value];
            this.CollectFragmentFields(runtimeType, fragment, fields);
        }

        private bool DoesFragmentConditionMatch(GraphQLComplexType runtimeType, GraphQLInlineFragment fragment)
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

        private bool ShouldIncludeNode(
            IEnumerable<GraphQLDirective> directives,
            DirectiveLocation location)
        {
            foreach (var directive in directives)
            {
                var directiveType = this.schemaRepository.GetDirective(directive.Name.Value);
                if (directiveType != null)
                {
                    if (!directiveType.Locations.Any(e => e == location))
                        continue;
                    if (!directiveType.PreExecutionIncludeFieldIntoResult(directive, this.schemaRepository))
                        return false;
                }
            }

            return true;
        }
    }
}