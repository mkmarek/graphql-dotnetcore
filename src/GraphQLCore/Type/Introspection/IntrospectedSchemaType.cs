namespace GraphQLCore.Type.Introspection
{
    using System.Collections.Generic;
    using System.Linq;
    using Translation;

    public class IntrospectedSchemaType : GraphQLObjectType
    {
        private IGraphQLSchema schema;
        private ISchemaRepository schemaRepository;

        public IntrospectedSchemaType(
            ISchemaRepository schemaRepository,
            IGraphQLSchema schema) : base(
            "__Schema",
            "A GraphQL Schema defines the capabilities of a GraphQL server. It " +
            "exposes all available types and directives on the server, as well as " +
            "the entry points for query, mutation, and subscription operations.")
        {
            this.schemaRepository = schemaRepository;
            this.schema = schema;

            this.Field("types", () => this.IntrospectAllSchemaTypes()).WithDescription(
                "A list of all types supported by this server.");

            this.Field("queryType", () => this.IntrospectQueryType()).WithDescription(
                "The type that query operations will be rooted at.");

            this.Field("mutationType", () => this.IntrospectMutationType()).WithDescription(
                "If this server supports mutation, the type that " +
                "mutation operations will be rooted at.");

            this.Field("subscriptionType", () => this.IntrospectSubscriptionType()).WithDescription(
                "If this server support subscription, the type that " +
                "subscription operations will be rooted at.");

            this.Field("directives", () => this.IntrospectDirectives()).WithDescription(
                "A list of all directives supported by this server.");
        }

        public IEnumerable<IntrospectedDirective> IntrospectDirectives()
        {
            return this.schemaRepository.GetDirectives()
                .Select(e => e.Introspect(this.schemaRepository));
        }

        public IEnumerable<IntrospectedType> IntrospectAllSchemaTypes()
        {
            var result = new List<IntrospectedType>();

            foreach (var type in this.schemaRepository.GetOutputKnownTypes())
                result.Add(type.Introspect(this.schemaRepository));

            foreach (var type in this.schemaRepository.GetInputKnownTypes().Where(e => !result.Any(r => r.Name == e.Name)))
                result.Add(type.Introspect(this.schemaRepository));

            return result
                .Where(e => e.Name != null)
                .OrderBy(e => e.Name)
                .ToList();
        }

        private IntrospectedType IntrospectMutationType()
        {
            return this.schema.MutationType?.Introspect(this.schemaRepository);
        }

        private IntrospectedType IntrospectSubscriptionType()
        {
            return this.schema.SubscriptionType?.Introspect(this.schemaRepository);
        }

        private IntrospectedType IntrospectQueryType()
        {
            return this.schema.QueryType?.Introspect(this.schemaRepository);
        }
    }
}