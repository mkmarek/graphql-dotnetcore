using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLUnionTypeDefinition : GraphQLTypeDefinition
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.UnionTypeDefinition;
            }
        }

        public GraphQLName Name { get; set; }
        public IEnumerable<GraphQLNamedType> Types { get; set; }
    }
}
