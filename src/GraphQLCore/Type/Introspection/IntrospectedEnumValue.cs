namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedEnumValue
    {
        public NonNullable<string> Name { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }
    }
}