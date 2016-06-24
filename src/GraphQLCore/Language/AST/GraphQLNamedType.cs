using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Language.AST
{
    public class GraphQLNamedType : GraphQLType
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.NamedType;
            }
        }

        public GraphQLName Name { get; set; }
    }
}
