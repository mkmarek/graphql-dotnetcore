using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Language.AST
{
    public class GraphQLObjectTypeDefinition : ASTNode
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public IEnumerable<GraphQLFieldDefinition> Fields { get; set; }

        public IEnumerable<GraphQLNamedType> Interfaces { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ObjectTypeDefinition;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
