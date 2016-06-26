namespace GraphQLCore.Type
{
    using System.Linq.Expressions;
    using Utils;

    public class GraphQLInputArgument : GraphQLObjectType
    {
        public GraphQLInputArgument(ParameterExpression parameter) : base("GraphQLArgument", "", null)
        {
            this.Field("name", () => parameter.Name);
            this.Field("description", () => null as string);
            this.Field("defaultValue", () => null as object);
            this.Field("type", () => TypeUtilities.ResolveObjectFieldType(parameter.Type));
        }
    }
}