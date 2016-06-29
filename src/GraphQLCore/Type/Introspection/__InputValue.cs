namespace GraphQLCore.Type
{
    using System.Linq.Expressions;
    using Utils;

    public class __InputValue : GraphQLObjectType
    {
        public __InputValue(ParameterExpression parameter, GraphQLSchema schema) : base("__InputValue", "", null)
        {
            this.schema = schema;

            this.Field("name", () => parameter.Name);
            this.Field("description", () => null as string);
            this.Field("defaultValue", () => null as string);
            this.Field("type", () => TypeUtilities.ResolveObjectFieldType(parameter.Type, this.schema));
        }
    }
}