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

        public NonNullable<IEnumerable<NonNullable<IntrospectedDirective>>> IntrospectDirectives()
        {
            var directives = this.schemaRepository.GetDirectives();
            if (directives == null)
                return new NonNullable<IEnumerable<NonNullable<IntrospectedDirective>>>(
                    Enumerable.Empty<NonNullable<IntrospectedDirective>>());

            return this.schemaRepository.GetDirectives().Select(e =>
                (NonNullable<IntrospectedDirective>)e.Introspect(this.schemaRepository)).ToList();
        }

        public NonNullable<IEnumerable<NonNullable<IntrospectedType>>> IntrospectAllSchemaTypes()
        {
            var result = new List<NonNullable<IntrospectedType>>();

            foreach (var type in this.schemaRepository.GetAllKnownTypes())
                result.Add(type.Introspect(this.schemaRepository));

            return result
                .OrderBy(e => e.Value.Name)
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

        private NonNullable<IntrospectedType> IntrospectQueryType()
        {
            return this.schema.QueryType.Introspect(this.schemaRepository);
        }
    }
}