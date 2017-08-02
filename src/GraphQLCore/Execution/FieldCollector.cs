namespace GraphQLCore.Execution
{
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;
    using Language.AST;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Threading.Tasks;
    using Type;
    using Utils;

    public class FieldCollector : IFieldCollector
    {
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private ISchemaRepository schemaRepository;
        public Queue<FieldExecution> PostponedFieldQueue { get; }

        public FieldCollector(
            Dictionary<string, GraphQLFragmentDefinition> fragments,
            ISchemaRepository schemaRepository)
        {
            this.fragments = fragments;
            this.schemaRepository = schemaRepository;
            this.PostponedFieldQueue = new Queue<FieldExecution>();
        }

        public Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(
            GraphQLComplexType runtimeType, GraphQLSelectionSet selectionSet, FieldScope scope)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields, scope);

            return fields;
        }

        protected virtual void CollectField(GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields, FieldScope scope)
        {
            if (!this.ShouldIncludeNode(selection.Directives, DirectiveLocation.FIELD, scope, selection))
                return;

            var entryKey = this.GetFieldEntryKey(selection);

            if (!fields.ContainsKey(entryKey))
                fields.Add(entryKey, new List<GraphQLFieldSelection>());

            fields[entryKey].Add(selection);
        }

        private void CollectFieldsInSelection(GraphQLComplexType runtimeType, ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields, FieldScope scope)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields, scope); break;
                case ASTNodeKind.FragmentSpread: this.CollectFragmentSpreadFields(runtimeType, (GraphQLFragmentSpread)selection, fields, scope); break;
                case ASTNodeKind.InlineFragment: this.CollectFragmentFields(runtimeType, (GraphQLInlineFragment)selection, fields, scope); break;
            }
        }

        private void CollectFragmentFields(GraphQLComplexType runtimeType, GraphQLInlineFragment fragment, Dictionary<string, IList<GraphQLFieldSelection>> fields, FieldScope scope)
        {
            if (!this.ShouldIncludeNode(fragment.Directives, DirectiveLocation.INLINE_FRAGMENT, scope, fragment))
                return;

            if (!this.DoesFragmentConditionMatch(runtimeType, fragment))
                return;

            this.CollectFields(runtimeType, fragment.SelectionSet, scope)
                .ToList().ForEach(e => fields.Add(e.Key, e.Value));
        }

        private void CollectFragmentSpreadFields(GraphQLComplexType runtimeType, GraphQLFragmentSpread fragmentSpread, Dictionary<string, IList<GraphQLFieldSelection>> fields, FieldScope scope)
        {
            if (!this.ShouldIncludeNode(fragmentSpread.Directives, DirectiveLocation.FRAGMENT_SPREAD, scope, fragmentSpread))
                return;

            var fragment = this.fragments[fragmentSpread.Name.Value];
            this.CollectFragmentFields(runtimeType, fragment, fields, scope);
        }

        private bool DoesFragmentConditionMatch(GraphQLComplexType runtimeType, GraphQLInlineFragment fragment)
        {
            if (fragment.TypeCondition == null)
                return true;

            var type = this.schemaRepository.GetSchemaOutputTypeByName(fragment.TypeCondition.Name.Value);
            if (type == runtimeType || TypeComparators.IsPossibleType(runtimeType, type, this.schemaRepository))
                return true;

            return false;
        }

        private string GetFieldEntryKey(GraphQLFieldSelection selection)
        {
            return selection.Alias != null ? selection.Alias.Value : selection.Name.Value;
        }

        private bool ShouldIncludeNode(
            IEnumerable<GraphQLDirective> directives,
            DirectiveLocation location,
            FieldScope scope,
            IWithDirectives node)
        {
            foreach (var directive in directives)
            {
                var directiveType = this.schemaRepository.GetDirective(directive.Name.Value);
                if (directiveType != null)
                {
                    if (!directiveType.Locations.Any(e => e == location))
                        continue;
                    IEnumerable<Task<dynamic>> postponedExecutions;
                    if (directiveType.PostponeNodeResolve(scope, node, out postponedExecutions))
                    {
                        foreach (var execution in postponedExecutions)
                        {
                            this.PostponedFieldQueue.Enqueue(new FieldExecution()
                            {
                                Path = scope.Path.Append(((GraphQLFieldSelection)node).GetPathName()),
                                Result = execution
                            });
                        }
                    }

                    if (!directiveType.PreExecutionIncludeFieldIntoResult(directive, this.schemaRepository))
                        return false;
                }
            }

            return true;
        }
    }
}