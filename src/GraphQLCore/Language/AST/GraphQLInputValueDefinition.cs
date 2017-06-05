using System.Collections.Generic;

namespace GraphQLCore.Language.AST
{
    public class GraphQLInputValueDefinition : GraphQLTypeDefinition, IWithDirectives
    {
        public GraphQLValue DefaultValue { get; set; }

        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.InputValueDefinition;
            }
        }

        public GraphQLName Name { get; set; }
        public GraphQLType Type { get; set; }
    }
}