using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Parser.Language.AST
{
    public class GraphQLValue<T> : GraphQLValue
    {
        private ASTNodeKind KindField;

        public override ASTNodeKind Kind
        {
            get
            {
                return this.KindField;
            }
        }

        public T Value { get; set; }

        public GraphQLValue(ASTNodeKind kind)
        {
            this.KindField = kind;
        }
    }
}
