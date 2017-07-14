namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedInputValue
    {
        public NonNullable<string> Name { get; set; }
        public string Description { get; set; }
        public NonNullable<IntrospectedType> Type { get; set; }
        public string DefaultValue { get; set; }
    }
}