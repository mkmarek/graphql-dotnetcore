namespace GraphQLCore.Type
{
    public class GraphQLEnumValue : GraphQLObjectType
    {
        public GraphQLEnumValue(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            this.Field("name", () => name);
            this.Field("description", () => description);
            this.Field("isDeprecated", () => null as bool?);
            this.Field("deprecationReason", () => null as string);
        }
    }
}