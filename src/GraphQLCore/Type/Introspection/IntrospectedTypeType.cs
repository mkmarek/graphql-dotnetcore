namespace GraphQLCore.Type.Introspection
{
    using Execution;

    public class IntrospectedTypeType : GraphQLObjectType<IntrospectedType>
    {
        public IntrospectedTypeType() : base(
            "__Type",
            "The fundamental unit of any GraphQL Schema is the type. There are " +
            "many kinds of types in GraphQL as represented by the `__TypeKind` enum." +
            "\n\nDepending on the kind of a type, certain fields describe " +
            "information about that type. Scalar types provide no information " +
            "beyond a name and description, while Enum types provide their values. " +
            "Object and Interface types provide the fields they describe. Abstract " +
            "types, Union and Interface, provide the Object types possible " +
            "at runtime. List and NonNull types compose other types.")
        {
            this.Field("kind", e => e.Kind);
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("fields", (IContext<IntrospectedType> type, bool? includeDeprecated) =>
                type.Instance.GetFields(includeDeprecated.Value))
                .WithDefaultValue("includeDeprecated", false);
            this.Field("interfaces", e => e.Interfaces);
            this.Field("possibleTypes", e => e.PossibleTypes);
            this.Field("enumValues", (IContext<IntrospectedType> type, bool? includeDeprecated) =>
                type.Instance.GetEnumValues(includeDeprecated.Value))
                .WithDefaultValue("includeDeprecated", false);
            this.Field("inputFields", e => e.InputFields);
            this.Field("ofType", e => e.OfType);
        }
    }
}