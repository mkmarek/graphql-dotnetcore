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

        public override object GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.FloatValue)
                return ((GraphQLScalarValue)astValue).Value.ParseFloatOrGiveNull();

            if (astValue.Kind == ASTNodeKind.IntValue)
            {
                object integerValue = ((GraphQLScalarValue)astValue).Value.ParseIntOrGiveNull();

                if (integerValue == null)
                    return null;

                return Convert.ToSingle(integerValue);
            }

            return null;
        }
    }
}