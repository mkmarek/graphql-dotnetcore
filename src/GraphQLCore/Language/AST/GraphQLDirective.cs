using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Language.AST
{
    public class GraphQLDirective : ASTNode
    {
        public IEnumerable<GraphQLArgument> Arguments { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Directive;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
