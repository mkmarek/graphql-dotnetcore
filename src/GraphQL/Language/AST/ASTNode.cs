using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public abstract class ASTNode
    {
        public abstract ASTNodeKind Kind { get; }
        public GraphQLLocation Location { get; set; }
    }
}
