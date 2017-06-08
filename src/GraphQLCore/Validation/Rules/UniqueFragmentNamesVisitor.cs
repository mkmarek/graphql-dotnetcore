namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class UniqueFragmentNamesVisitor : ValidationASTVisitor
    {
        private Dictionary<string, GraphQLName> knownFragmentNames = new Dictionary<string, GraphQLName>();

        public UniqueFragmentNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            var fragmentName = node.Name.Value;

            if (this.knownFragmentNames.ContainsKey(fragmentName))
                this.Errors.Add(this.GetFragmentNameError(fragmentName,
                    new ASTNode[] { this.knownFragmentNames[fragmentName], node.Name }));
            else
                this.knownFragmentNames.Add(node.Name.Value, node.Name);

            return base.BeginVisitFragmentDefinition(node);
        }

        private GraphQLException GetFragmentNameError(string variableName, IEnumerable<ASTNode> nodes)
        {
            return new GraphQLException($"There can be only one fragment named \"{variableName}\".", nodes);
        }
    }
}
