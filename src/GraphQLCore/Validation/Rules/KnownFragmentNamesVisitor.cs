namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class KnownFragmentNamesVisitor : GraphQLAstVisitor
    {
        private Dictionary<string, GraphQLFragmentDefinition> fragmentDefinitions;

        public KnownFragmentNamesVisitor(IGraphQLSchema schema)
        {
            this.Errors = new List<GraphQLException>();
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

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread node)
        {
            if (this.GetFragment(node.Name.Value) == null)
                this.Errors.Add(new GraphQLException(this.GetErrorMessage(node.Name.Value)));

            return base.BeginVisitFragmentSpread(node);
        }

        private GraphQLFragmentDefinition GetFragment(string name)
        {
            if (this.fragmentDefinitions.ContainsKey(name))
                return this.fragmentDefinitions[name];

            return null;
        }

        private string GetErrorMessage(string fragmentName)
        {
            return $"Unknown fragment \"{fragmentName}\".";
        }
    }
}