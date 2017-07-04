namespace GraphQLCore.Type.Scalar
{
    using Language.AST;
    using System;
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

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (value is int)
                value = Convert.ToBoolean(value);

            if (!(value is bool))
                return null;

            var stringValue = (bool)value ? "true" : "false";

            return new GraphQLScalarValue(ASTNodeKind.BooleanValue)
            {
                Value = stringValue
            };
        }
    }
}