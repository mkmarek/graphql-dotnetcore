using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLName : ASTNode
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Name;
            }
        }

        public string Value { get; internal set; }
    }
}
