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

        public override object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.BooleanValue)
                return ((GraphQLScalarValue)astValue).Value.ParseBoolOrGiveNull();

            return null;
        }
    }
}