namespace GraphQLCore.Type.Introspection
{
    using GraphQLCore.Type.Directives;

    public class IntrospectedDirectiveLocationType : GraphQLEnumType<DirectiveLocation>
    {
        public IntrospectedDirectiveLocationType() : base(
            "__DirectiveLocation",
            string.Empty)
        {
        }
    }
}