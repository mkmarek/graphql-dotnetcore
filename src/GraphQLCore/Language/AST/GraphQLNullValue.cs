namespace GraphQLCore.Language.AST
{
    public class GraphQLNullValue : GraphQLValue
    {
        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.NullValue;
            }
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
