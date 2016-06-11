using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLDocument : ASTNode
    {
        public IEnumerable<ASTNode> Definitions { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Document;
            }
        }
    }
}
