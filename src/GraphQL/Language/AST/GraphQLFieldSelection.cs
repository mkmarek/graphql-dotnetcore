using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLFieldSelection : ASTNode
    {
        public GraphQLName Alias { get; set; }

        public IEnumerable<GraphQLArgument> Arguments { get; set; }

        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Field;
            }
        }

        public GraphQLName Name { get; set; }
        public GraphQLSelectionSet SelectionSet { get; set; }
    }

}
