namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System;
    using Translation;
    public class GraphQLLong : GraphQLScalarType
    {
        public GraphQLLong() : base(
            "Long",
            "The `Long` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^63) and 2^63 - 1.")
        {
        }

        public override object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.IntValue)
            {
                decimal value;
                if (!decimal.TryParse(((GraphQLScalarValue)astValue).Value, out value))
                    return null;

                if (value <= long.MaxValue && value >= long.MinValue)
                {
                    return Convert.ToInt64(value);
                }
            }

            return null;
        }
    }
}