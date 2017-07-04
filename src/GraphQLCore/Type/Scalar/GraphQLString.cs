namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System.Text.RegularExpressions;
    using Translation;
    using Utils;

    public class GraphQLString : GraphQLScalarType
    {
        public GraphQLString() : base(
            "String",
            "The `String` scalar type represents textual data, represented as UTF-8 " +
            "character sequences. The String type is most often used by GraphQL to " +
            "represent free-form human-readable text.")
        {
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.StringValue)
                return new Result(((GraphQLScalarValue)astValue).Value);

            return Result.Invalid;
        }

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            return new GraphQLScalarValue(ASTNodeKind.StringValue)
            {
                Value = value.ParseString()
            };
        }
    }
}