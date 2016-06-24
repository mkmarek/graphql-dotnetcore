using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Language.AST
{
    public class GraphQLTypeExtensionDefinition : GraphQLTypeDefinition
    {
        public GraphQLObjectTypeDefinition Definition { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.TypeExtensionDefinition;
            }
        }
    }
}
