namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System;
    using Translation;
    using Utils;

    public class GraphQLInt : GraphQLScalarType
    {
        public GraphQLInt() : base(
            "Int",
            "The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.")
        {
        }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.IntValue)
            {
                decimal value;
                if (!decimal.TryParse(((GraphQLScalarValue)astValue).Value, out value))
                    return Result.Invalid;

                if (value <= int.MaxValue && value >= int.MinValue)
                    return new Result(Convert.ToInt32(value));
            }

            return Result.Invalid;
        }

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (value is float || value is double)
                value = value.ToString().ParseIntOrGiveNull();

            if (!(value is int))
                return null;

            return new GraphQLScalarValue(ASTNodeKind.IntValue)
            {
                Value = value.ToString()
            };
        }
    }
}