using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLFragmentDefinition : ASTNode
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.FragmentDefinition;
            }
        }

        public GraphQLName Name { get; set; }
        public GraphQLSelectionSet SelectionSet { get; set; }
        public GraphQLNamedType TypeCondition { get; set; }
    }
}
