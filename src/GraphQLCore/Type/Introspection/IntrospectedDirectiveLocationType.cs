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
            this.EnumValue(DirectiveLocation.QUERY)
                .WithDescription("Location adjacent to a query operation.");
            this.EnumValue(DirectiveLocation.MUTATION)
                .WithDescription("Location adjacent to a mutation operation.");
            this.EnumValue(DirectiveLocation.SUBSCRIPTION)
                .WithDescription("Location adjacent to a subscription operation.");
            this.EnumValue(DirectiveLocation.FIELD)
                .WithDescription("Location adjacent to a field.");
            this.EnumValue(DirectiveLocation.FRAGMENT_DEFINITION)
                .WithDescription("Location adjacent to a fragment definition.");
            this.EnumValue(DirectiveLocation.FRAGMENT_SPREAD)
                .WithDescription("Location adjacent to a fragment spread.");
            this.EnumValue(DirectiveLocation.INLINE_FRAGMENT)
                .WithDescription("Location adjacent to an inline fragment.");
            this.EnumValue(DirectiveLocation.SCHEMA)
                .WithDescription("Location adjacent to a schema definition.");
            this.EnumValue(DirectiveLocation.SCALAR)
                .WithDescription("Location adjacent to a scalar definition.");
            this.EnumValue(DirectiveLocation.OBJECT)
                .WithDescription("Location adjacent to an object type definition.");
            this.EnumValue(DirectiveLocation.FIELD_DEFINITION)
                .WithDescription("Location adjacent to a field definition.");
            this.EnumValue(DirectiveLocation.ARGUMENT_DEFINITION)
                .WithDescription("Location adjacent to an argument definition.");
            this.EnumValue(DirectiveLocation.INTERFACE)
                .WithDescription("Location adjacent to an interface definition.");
            this.EnumValue(DirectiveLocation.UNION)
                .WithDescription("Location adjacent to a union definition.");
            this.EnumValue(DirectiveLocation.ENUM)
                .WithDescription("Location adjacent to an enum definition.");
            this.EnumValue(DirectiveLocation.ENUM_VALUE)
                .WithDescription("Location adjacent to an enum value definition.");
            this.EnumValue(DirectiveLocation.INPUT_OBJECT)
                .WithDescription("Location adjacent to an input object type definition.");
            this.EnumValue(DirectiveLocation.INPUT_FIELD_DEFINITION)
                .WithDescription("Location adjacent to an input object field definition.");
        }
    }
}