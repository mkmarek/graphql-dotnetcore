using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLDirectiveDefinition : GraphQLTypeDefinition
    {
        public IEnumerable<GraphQLInputValueDefinition> Definitions { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.DirectiveDefinition;
            }
        }

        public IEnumerable<GraphQLName> Locations { get; set; }
        public IEnumerable<GraphQLInputValueDefinition> Arguments { get; set; }
        public GraphQLName Name { get; set; }
    }
}
