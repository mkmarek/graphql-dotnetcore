using System.Collections.Generic;

namespace GraphQLCore.Language.AST
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