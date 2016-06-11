using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLSelectionSet : ASTNode
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.SelectionSet;
            }
        }

        public IEnumerable<ASTNode> Selections { get; set; }
    }
}
