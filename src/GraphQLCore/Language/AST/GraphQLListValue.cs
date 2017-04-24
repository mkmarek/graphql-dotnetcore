namespace GraphQLCore.Language.AST
{
    using System.Collections.Generic;

    public class GraphQLListValue : GraphQLValue
    {
        private ASTNodeKind kindField;

        public GraphQLListValue(ASTNodeKind kind)
        {
            this.kindField = kind;
        }

        public override ASTNodeKind Kind
        {
            get
            {
                return this.kindField;
            }
        }

        public IEnumerable<GraphQLValue> Values { get; set; }

        public override string ToString()
        {
            var values = string.Join(", ", this.Values);

            return $"[{values}]";
        }
    }
}
