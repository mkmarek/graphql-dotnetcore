using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLListType : GraphQLType
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ListType;
            }
        }

        public GraphQLType Type { get; set; }
    }
}
