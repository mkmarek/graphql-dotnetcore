namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedDirectiveType : GraphQLObjectType<IntrospectedDirective>
    {
        public IntrospectedDirectiveType() : base(
            "__Directive",
            "A Directive provides a way to describe alternate runtime execution " +
            "and type validation behavior in a GraphQL document.\n\n" +
            "In some cases, you need to provide options to alter GraphQL's " +
            "execution behavior in ways field arguments will not suffice, such as " +
            "conditionally including or skipping a field. Directives provide this by " +
            "describing additional information to the executor.")
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("locations", e => e.Locations);
            this.Field("args", e => e.Arguments);
        }
    }
}