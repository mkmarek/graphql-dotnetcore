namespace GraphQLCore.Validation.Rules
{
    using Abstract;
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class NoFragmentCyclesVisitor : VariableValidationVisitor
    {
        private List<string> visitedFragments;
        private Stack<GraphQLFragmentSpread> spreadPath;
        private Dictionary<string, int> spreadPathIndexByName;
        private Dictionary<string, GraphQLFragmentDefinition> fragmentDefinitions;

        public NoFragmentCyclesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
            this.visitedFragments = new List<string>();
            this.spreadPath = new Stack<GraphQLFragmentSpread>();
            this.spreadPathIndexByName = new Dictionary<string, int>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override void Visit(GraphQLDocument document)
        {
            this.fragmentDefinitions = document.Definitions
                .Where(e => e.Kind == ASTNodeKind.FragmentDefinition)
                .Cast<GraphQLFragmentDefinition>()
                .ToDictionary(e => e.Name.Value, e => e);

            base.Visit(document);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition node)
        {
            return node;
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            if (!this.visitedFragments.Any(e => e == node.Name.Value))
            {
                this.DetectCycleRecursive(node);
            }

            return node;
        }

        private void DetectCycleRecursive(GraphQLFragmentDefinition node)
        {
            var fragmentName = node.Name.Value;
            this.visitedFragments.Add(fragmentName);

            var spreadNodes = this.GetFragmentSpreads(node.SelectionSet);

            if (!spreadNodes.Any())
                return;

            this.spreadPathIndexByName.Add(fragmentName, this.spreadPath.Count());

            foreach (var spreadNode in spreadNodes)
            {
                var spreadName = spreadNode.Name.Value;
                var cycleIndex = this.spreadPathIndexByName.ContainsKey(spreadName)
                    ? this.spreadPathIndexByName[spreadName]
                    : null as int?;

                if (!cycleIndex.HasValue)
                {
                    this.spreadPath.Push(spreadNode);
                    if (!this.visitedFragments.Contains(spreadName))
                    {
                        var spreadFragment = this.GetFragment(spreadName);

                        if (spreadFragment != null)
                            this.DetectCycleRecursive(spreadFragment);
                    }

                    this.spreadPath.Pop();
                }
                else
                {
                    var cyclePath = this.spreadPath
                        .Reverse()
                        .Skip(cycleIndex.Value);

                    this.Errors.Add(new GraphQLException(
                        this.GetErrorMessage(
                            spreadName,
                            cyclePath.Select(e => e.Name.Value))));
                }
            }

            this.spreadPathIndexByName.Remove(fragmentName);
        }

        private IEnumerable<GraphQLFragmentSpread> GetFragmentSpreads(GraphQLSelectionSet selectionSet)
        {
            if (selectionSet == null || selectionSet.Selections == null)
                return new GraphQLFragmentSpread[] { };

            var spreads = selectionSet.Selections
                .Where(e => e.Kind == ASTNodeKind.FragmentSpread)
                .Cast<GraphQLFragmentSpread>()
                .ToList();

            var fields = selectionSet.Selections
                .Where(e => e.Kind == ASTNodeKind.Field)
                .Cast<GraphQLFieldSelection>()
                .ToList();

            foreach (var field in fields)
                spreads.AddRange(this.GetFragmentSpreads(field.SelectionSet));

            var inlineSpreads = selectionSet.Selections
                .Where(e => e.Kind == ASTNodeKind.InlineFragment)
                .Cast<GraphQLInlineFragment>()
                .ToList();

            foreach (var spread in inlineSpreads)
                spreads.AddRange(this.GetFragmentSpreads(spread.SelectionSet));

            return spreads;
        }

        private GraphQLFragmentDefinition GetFragment(string name)
        {
            if (this.fragmentDefinitions.ContainsKey(name))
                return this.fragmentDefinitions[name];

            return null;
        }

        private string GetErrorMessage(string fragmentName, IEnumerable<string> spreadNames)
        {
            var via = spreadNames.Count() > 0 ? " via " + string.Join(", ", spreadNames) : string.Empty;
              return $"Cannot spread fragment \"{fragmentName}\" within itself{via}.";
        }
    }
}
