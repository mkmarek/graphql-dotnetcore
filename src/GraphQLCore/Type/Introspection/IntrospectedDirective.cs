namespace GraphQLCore.Type.Introspection
{
    using GraphQLCore.Type.Complex;
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Translation;
    using System.Linq;
    using System.Linq.Expressions;

    public class IntrospectedDirective
    {
        private ISchemaRepository schemaRepository;

        public string Name { get; set; }
        public string Description { get; set; }
        public DirectiveLocation[] Locations { get; set; }

        public LambdaExpression Resolver { get; set; }

        public IntrospectedInputValue[] GetArgs()
        {
            return this.Resolver.Parameters.Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                SystemType = e.Type
            })
            .Select(field => new IntrospectedInputValue()
            {
                Name = field.Name,
                Type = field.GetGraphQLType(this.schemaRepository)
                    .Introspect(this.schemaRepository)
            }).ToArray();
        }

        public IntrospectedDirective(ISchemaRepository schemaRepository)
        {
            this.schemaRepository = schemaRepository;
        }
    }
}