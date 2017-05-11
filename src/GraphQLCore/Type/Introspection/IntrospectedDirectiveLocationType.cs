namespace GraphQLCore.Type.Introspection
{
    using Directives;

    public class IntrospectedDirectiveLocationType : GraphQLEnumType<DirectiveLocation>
    {
        public IntrospectedDirectiveLocationType() : base(
            "__DirectiveLocation",
            "A Directive can be adjacent to many parts of the GraphQL language, " +
            "a __DirectiveLocation describes one such possible adjacencies.")
        {
        }
    }
}