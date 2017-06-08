namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class NoUnusedFragmentsVisitor : GraphQLAstVisitor
    {
        private Dictionary<string, GraphQLFragmentDefinition> fragmentDefinitions;

        public NoUnusedFragmentsVisitor(IGraphQLSchema schema)
        {
            this.fragmentDefinitions = new Dictionary<string, GraphQLFragmentDefinition>();
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            var key = fragmentSpread.Name.Value;

            if (this.fragmentDefinitions.ContainsKey(key))
            {
                var definition = this.fragmentDefinitions[key];
                this.fragmentDefinitions.Remove(key);

                base.BeginVisitFragmentDefinition(definition);
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            return node;
        }

        public override void Visit(GraphQLDocument ast)
        {
            this.fragmentDefinitions = ast.Definitions
                .Where(e => e.Kind == ASTNodeKind.FragmentDefinition)
                .Cast<GraphQLFragmentDefinition>()
                .ToDictionary(e => e.Name.Value, e => e);

            base.Visit(ast);

            foreach (var unusedFragment in this.fragmentDefinitions)
            {
                this.Errors.Add(new GraphQLException($"Fragment \"{unusedFragment.Key}\" is never used.",
                    new[] { unusedFragment.Value }));
            }
        }
    }
}