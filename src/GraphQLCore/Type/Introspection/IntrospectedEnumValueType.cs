namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedEnumValueType : GraphQLObjectType<IntrospectedEnumValue>
    {
        public IntrospectedEnumValueType() : base(
            "__EnumValue",
            "One possible value for a given Enum. Enum values are unique " +
            "values, not a placeholder for a string or numeric value. However an " +
            "Enum value is returned in a JSON response as a string.")
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("isDeprecated", e => e.IsDeprecated);
            this.Field("deprecationReason", e => e.DeprecationReason);
        }
    }
}