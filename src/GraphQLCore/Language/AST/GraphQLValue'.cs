namespace GraphQLCore.Language.AST
{
    public class GraphQLValue<T> : GraphQLValue
    {
        private ASTNodeKind KindField;

        public GraphQLValue(ASTNodeKind kind)
        {
            this.KindField = kind;
        }

        public override ASTNodeKind Kind
        {
            get
            {
                return this.KindField;
            }
        }

        public T Value { get; set; }
    }
}