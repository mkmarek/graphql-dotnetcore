using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLInterfaceTypeDefinition : GraphQLTypeDefinition
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public IEnumerable<GraphQLFieldDefinition> Fields { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.InterfaceTypeDefinition;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
