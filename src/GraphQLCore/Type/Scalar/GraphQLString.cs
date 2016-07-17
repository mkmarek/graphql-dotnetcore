namespace GraphQLCore.Type.Scalar
{
    using Language.AST;

    public class GraphQLString : GraphQLScalarType
    {
        public GraphQLString() : base(
            "String",
            "The `String` scalar type represents textual data, represented as UTF-8 " +
            "character sequences. The String type is most often used by GraphQL to " +
            "represent free-form human-readable text.")
        {
        }

        public override object GetFromAst(GraphQLValue astValue)
        {
            if (astValue.Kind == ASTNodeKind.StringValue)
                return ((GraphQLScalarValue)astValue).Value;

            return null;
        }
    }
}