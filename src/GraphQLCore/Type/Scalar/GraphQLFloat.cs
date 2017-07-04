namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System;
    using System.Globalization;
    using Translation;
    using Utils;

    public class GraphQLFloat : GraphQLScalarType
    {
        public GraphQLFloat() : base(
            "Float",
            "The `Float` scalar type represents signed double-precision fractional " +
            "values as specified by " +
            "[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point). ")
        {
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.FloatValue || astValue.Kind == ASTNodeKind.IntValue)
            {
                var value = ((GraphQLScalarValue)astValue).Value.ParseFloatOrGiveNull();
                if (value != null)
                    return new Result(value);
            }

            return Result.Invalid;
        }

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (!(value is int) && !(value is float) && !(value is double))
                return null;

            var stringValue = value.ToString();

            var intValue = stringValue.ParseIntOrGiveNull();
            if (intValue != null)
                return new GraphQLScalarValue(ASTNodeKind.IntValue)
                {
                    Value = intValue.ToString()
                };

            return new GraphQLScalarValue(ASTNodeKind.FloatValue)
            {
                Value = ((double)value).ToString(CultureInfo.InvariantCulture).ToLower()
            };
        }
    }
}