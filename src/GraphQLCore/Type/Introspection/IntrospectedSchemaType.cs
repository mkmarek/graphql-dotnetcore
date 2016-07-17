namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;
    using Translation;

    public class IntrospectedSchemaType : GraphQLObjectType
    {
        private IGraphQLSchema schema;
        private ISchemaObserver schemaObserver;

        public IntrospectedSchemaType(
            ISchemaObserver schemaObserver,
            IGraphQLSchema schema) : base(
            "__Schema",
            "A GraphQL Schema defines the capabilities of a GraphQL server. It " +
            "exposes all available types and directives on the server, as well as " +
            "the entry points for query, mutation, and subscription operations.")
        {
            this.schemaObserver = schemaObserver;
            this.schema = schema;

            this.Field("types", () => this.IntrospectAllSchemaTypes());
            this.Field("queryType", () => this.schema.QueryType.Introspect(this.schemaObserver));
            this.Field("mutationType", () => this.schema.MutationType.Introspect(this.schemaObserver));
        }

        public IEnumerable<IntrospectedType> IntrospectAllSchemaTypes()
        {
            var result = new List<IntrospectedType>();

            foreach (var type in this.schemaObserver.GetOutputKnownTypes())
                result.Add(type.Introspect(this.schemaObserver));

            foreach (var type in this.schemaObserver.GetInputKnownTypes().Where(e => !result.Any(r => r.Name == e.Name)))
                result.Add(type.Introspect(this.schemaObserver));

            return result
                .Where(e => e.Name != null)
                .OrderBy(e => e.Name)
                .ToList();
        }
    }
}