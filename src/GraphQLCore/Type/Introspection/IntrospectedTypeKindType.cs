namespace GraphQLCore.Type.Introspection
{
    public enum TypeKind
    {
        SCALAR,
        OBJECT,
        INTERFACE,
        UNION,
        ENUM,
        INPUT_OBJECT,
        LIST,
        NON_NULL
    }

    public class IntrospectedTypeKindType : GraphQLEnumType<TypeKind>
    {
        public IntrospectedTypeKindType() : base(
            "__TypeKind",
            "An enum describing what kind of type a given `__Type` is.")
        {
            this.EnumValue(TypeKind.SCALAR)
                .WithDescription("Indicates this type is a scalar.");
            this.EnumValue(TypeKind.OBJECT)
                .WithDescription("Indicates this type is an object. `fields` and `interfaces` are valid fields.");
            this.EnumValue(TypeKind.INTERFACE)
                .WithDescription("Indicates this type is an interface. `fields` and `possibleTypes` are valid fields.");
            this.EnumValue(TypeKind.UNION)
                .WithDescription("Indicates this type is a union. `possibleTypes` is a valid field.");
            this.EnumValue(TypeKind.ENUM)
                .WithDescription("Indicates this type is an enum. `enumValues` is a valid field.");
            this.EnumValue(TypeKind.INPUT_OBJECT)
                .WithDescription("Indicates this type is an input object. `inputFields` is a valid field.");
            this.EnumValue(TypeKind.LIST)
                .WithDescription("Indicates this type is a list. `ofType` is a valid field.");
            this.EnumValue(TypeKind.NON_NULL)
                .WithDescription("Indicates this type is a non-null. `ofType` is a valid field.");
        }
    }
}