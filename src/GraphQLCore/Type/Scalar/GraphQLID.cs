namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System.Text.RegularExpressions;
    using Translation;
    using Utils;

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

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (value is bool)
                value = value.ToString().ToLower();

            var stringValue = value.ToString();
            if (stringValue.ParseIntOrGiveNull() != null)
                return new GraphQLScalarValue(ASTNodeKind.IntValue)
                {
                    Value = stringValue
                };

            return new GraphQLScalarValue(ASTNodeKind.StringValue)
            {
                Value = Regex.Escape(stringValue)
            };
        }
    }
}
