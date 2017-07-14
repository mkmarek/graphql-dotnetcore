namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedField
    {
        public NonNullable<NonNullable<IntrospectedInputValue>[]> Arguments { get; set; }
        public string DeprecationReason { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public NonNullable<string> Name { get; set; }
        public NonNullable<IntrospectedType> Type { get; set; }
    }
}