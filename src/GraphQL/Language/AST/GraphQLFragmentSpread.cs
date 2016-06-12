using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLFragmentSpread : ASTNode
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.FragmentSpread;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
