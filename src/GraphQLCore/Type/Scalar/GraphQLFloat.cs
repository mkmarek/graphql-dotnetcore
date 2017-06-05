namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System;
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
            if (astValue.Kind == ASTNodeKind.FloatValue)
                return new Result(((GraphQLScalarValue)astValue).Value.ParseFloatOrGiveNull());

            if (astValue.Kind == ASTNodeKind.IntValue)
            {
                object integerValue = ((GraphQLScalarValue)astValue).Value.ParseIntOrGiveNull();

                if (integerValue == null)
                    return new Result(null);

                return new Result(Convert.ToSingle(integerValue));
            }

            return Result.Invalid;
        }
    }
}