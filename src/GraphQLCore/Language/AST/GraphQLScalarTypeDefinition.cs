using System.Collections.Generic;

namespace GraphQLCore.Language.AST
{
    public class GraphQLScalarTypeDefinition : GraphQLTypeDefinition, IWithDirectives
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ScalarTypeDefinition;
            }
        }

        public GraphQLName Name { get; set; }
    }
}