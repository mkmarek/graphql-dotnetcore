namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedInputValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IntrospectedType Type { get; set; }
        public string DefaultValue { get; set; }
    }
}