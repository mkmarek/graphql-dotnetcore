namespace GraphQLCore.Type.Introspection
{
    using Directives;

    public class IntrospectedDirective
    {
        public NonNullable<string> Name { get; set; }
        public string Description { get; set; }
        public NonNullable<DirectiveLocation[]> Locations { get; set; }
        public NonNullable<NonNullable<IntrospectedInputValue>[]> Arguments { get; set; }
    }
}