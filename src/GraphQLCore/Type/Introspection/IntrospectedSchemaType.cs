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

            this.Field("types", () => this.IntrospectAllSchemaTypes());
            this.Field("queryType", () => this.IntrospectQueryType());
            this.Field("mutationType", () => this.IntrospectMudationType());
        }

        private IntrospectedType IntrospectMudationType()
        {
            return this.schema.MutationType?.Introspect(this.schemaRepository);
        }

        private IntrospectedType IntrospectQueryType()
        {
            return this.schema.QueryType?.Introspect(this.schemaRepository);
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
    }
}