namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using Translation;
    using Utils;

    public class GraphQLBoolean : GraphQLScalarType
    {
        public GraphQLBoolean() : base(
            "Boolean",
            "The `Boolean` scalar type represents `true` or `false`.")
        {
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.BooleanValue)
                return new Result(((GraphQLScalarValue)astValue).Value.ParseBoolOrGiveNull());

            return Result.Invalid;
        }
    }
}