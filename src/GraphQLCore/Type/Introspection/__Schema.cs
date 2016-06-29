namespace GraphQLCore.Type.Introspection
{
    public class __Schema : GraphQLObjectType
    {
        public __Schema(GraphQLSchema schema) : base("__Schema", "A GraphQL Schema defines the capabilities of a GraphQL server. It " +
            "exposes all available types and directives on the server, as well as " +
            "the entry points for query, mutation, and subscription operations.", schema)
        {
            this.Field("types", () => schema.Introspect());
            this.Field("queryType", () => new __Type(schema.RootType, schema));
        }
    }
}