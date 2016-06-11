using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLInlineFragment : ASTNode
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.InlineFragment;
            }
        }

        public GraphQLSelectionSet SelectionSet { get; set; }
        public GraphQLNamedType TypeCondition { get; set; }
    }
}
