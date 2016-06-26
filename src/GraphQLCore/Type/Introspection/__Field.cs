namespace GraphQLCore.Type.Introspection
{
    using System.Linq.Expressions;
    using Utils;

    public class __Field : GraphQLObjectType
    {
        public __Field(string fieldName, string fieldDescription, LambdaExpression expression) : base("__Field",
            "Object and Interface types are described by a list of Fields, each of " +
            "which has a name, potentially a list of arguments, and a return type."
            , null)
        {
            this.Field("name", () => fieldName);
            this.Field("description", () => fieldDescription);
            this.Field("isDeprecated", () => null as bool?);
            this.Field("deprecationReason", () => null as string);

            this.CreateTypeRelatedFields(expression);
        }

        private void CreateTypeRelatedFields(LambdaExpression expression)
        {
            var fieldType = ReflectionUtilities.GetReturnValueFromLambdaExpression(expression);

            this.Field("args", () => TypeUtilities.FetchInputArguments(expression));
            this.Field("type", () => TypeUtilities.ResolveObjectFieldType(fieldType));
        }
    }
}