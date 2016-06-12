using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLScalarTypeDefinition : GraphQLTypeDefinition
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ScalarTypeDefinition;
            }
        }

        public GraphQLName Name { get; set; }
        public IEnumerable<GraphQLDirective> Directives { get; set; }
    }
}
