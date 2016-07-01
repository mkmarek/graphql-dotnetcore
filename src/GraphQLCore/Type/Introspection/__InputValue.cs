namespace GraphQLCore.Type
{
    using Execution;
    using System.Linq.Expressions;

    public class __InputValue : GraphQLObjectType
    {
        public __InputValue(ParameterExpression parameter, GraphQLSchema schema) : base("__InputValue", "", null)
        {
            this.schema = schema;

            this.Field("name", () => parameter.Name);
            this.Field("description", () => null as string);
            this.Field("defaultValue", () => null as string);
            this.Field("type", () => TypeResolver.ResolveObjectArgumentType(parameter.Type, this.schema));
        }
    }
}