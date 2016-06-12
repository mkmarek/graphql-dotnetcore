using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Language.AST
{
    public class GraphQLObjectValue : GraphQLValue
    {
        public IEnumerable<GraphQLObjectField> Fields { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.ObjectValue;
            }
        }
    }
}
