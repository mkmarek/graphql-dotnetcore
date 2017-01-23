namespace GraphQLCore.Type.Directives
{
    using GraphQLCore.Type.Introspection;
    using GraphQLCore.Type.Translation;
    using Language.AST;
    using System.Linq.Expressions;

    public abstract class GraphQLDirectiveType
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DirectiveLocation[] Locations { get; private set; }

        protected GraphQLDirectiveType(
            string name,
            string description,
            params DirectiveLocation[] locations)
        {
            this.Name = name;
            this.Description = description;
            this.Locations = locations;
        }

        public virtual bool PreExecutionIncludeFieldIntoResult(
            GraphQLDirective directive, ISchemaRepository schemaRepository)
        {
            return true;
        }

        public virtual bool PostExecutionIncludeFieldIntoResult(
            GraphQLDirective directive,
            ISchemaRepository schemaRepository,
            object value,
            object parentValue)
        {
            return true;
        }

        public abstract LambdaExpression GetResolver(object value, object parentValue);

        public IntrospectedDirective Introspect(ISchemaRepository schemaRepository)
        {
            return new IntrospectedDirective(schemaRepository)
            {
                Name = this.Name,
                Description = this.Description,
                Locations = this.Locations,
                Resolver = this.GetResolver(null, null)
            };
        }
    }
}