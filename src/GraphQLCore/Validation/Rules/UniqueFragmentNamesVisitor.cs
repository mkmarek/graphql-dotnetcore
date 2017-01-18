namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class UniqueFragmentNamesVisitor : ValidationASTVisitor
    {
        public UniqueFragmentNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override void Visit(GraphQLDocument ast)
        {
            this.Errors = ast.Definitions
                .Where(x => x.Kind == ASTNodeKind.FragmentDefinition)
                .Select(x => ((GraphQLFragmentDefinition)x).Name.Value)
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key)
                .Select(this.GetFragmentNameError)
                .ToList();
        }

        private GraphQLException GetFragmentNameError(string variableName)
        {
            return new GraphQLException($"There can be only one fragment named \"{variableName}\".");
        }
    }
}
