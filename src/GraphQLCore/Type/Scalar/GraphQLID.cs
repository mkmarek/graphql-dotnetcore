namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using Translation;

    public class GraphQLID : GraphQLScalarType
    {
        public GraphQLID() : base(
            "ID",
            "The `ID` scalar type represents a unique identifier, often used to " +
            "refetch an object or as key for a cache. The ID type appears in a JSON " +
            "response as a String; however, it is not intended to be human-readable. " +
            "When expected as an input type, any string (such as `\"4\"`) or integer " +
            "(such as `4`) input value will be accepted as an ID.")
        {
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.StringValue || astValue.Kind == ASTNodeKind.IntValue)
                return new Result(((GraphQLScalarValue)astValue).Value);

            return Result.Invalid;
        }
    }
}
