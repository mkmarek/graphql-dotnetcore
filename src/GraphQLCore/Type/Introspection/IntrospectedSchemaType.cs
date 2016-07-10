namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;
    using Translation;

    public class IntrospectedSchemaType : GraphQLObjectType
    {
        private IIntrospector introspector;
        private IGraphQLSchema schema;
        private ISchemaObserver schemaObserver;

        public IntrospectedSchemaType(
            ISchemaObserver schemaObserver,
            IIntrospector introspector,
            IGraphQLSchema schema) : base(
            "__Schema",
            "A GraphQL Schema defines the capabilities of a GraphQL server. It " +
            "exposes all available types and directives on the server, as well as " +
            "the entry points for query, mutation, and subscription operations.")
        {
            this.introspector = introspector;
            this.schemaObserver = schemaObserver;
            this.schema = schema;

            this.Field("types", () => this.Introspect());
            this.Field("queryType", () => introspector.Introspect(this.schema.QueryType));
            this.Field("mutationType", () => introspector.Introspect(this.schema.MutationType));
        }

        public IEnumerable<IntrospectedType> Introspect()
        {
            var result = new List<IntrospectedType>();

            foreach (var type in this.schemaObserver.GetOutputKnownTypes())
                result.Add(this.introspector.Introspect(type));

            foreach (var type in this.schemaObserver.GetInputKnownTypes().Where(e => !result.Any(r => r.Name == e.Name)))
                result.Add(this.introspector.Introspect(type));

            return result
                .Where(e => e.Name != null)
                .OrderBy(e => e.Name)
                .ToList();
        }
    }
}