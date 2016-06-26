using System.Collections.Generic;

namespace GraphQLCore.Language.AST
{
    public class GraphQLEnumValueDefinition : GraphQLTypeDefinition
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.EnumValueDefinition;
            }
        }

        public GraphQLName Name { get; set; }
    }
}