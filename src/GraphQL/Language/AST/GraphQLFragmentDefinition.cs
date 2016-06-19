using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLFragmentDefinition : GraphQLInlineFragment
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.FragmentDefinition;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
