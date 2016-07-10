namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedField
    {
        public IntrospectedInputValue[] Arguments { get; set; }
        public string DeprecationReason { get; set; }
        public string Description { get; set; }
        public bool? IsDeprecated { get; set; }
        public string Name { get; set; }
        public IntrospectedType Type { get; set; }
    }
}