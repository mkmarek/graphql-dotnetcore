using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLVariable : GraphQLValue
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Variable;
            }
        }
        public GraphQLName Name { get; set; }
    }
}
