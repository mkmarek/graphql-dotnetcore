namespace GraphQLCore.Type
{
    using Introspection;

    public class IntrospectedInputValueType : GraphQLObjectType<IntrospectedInputValue>
    {
        public IntrospectedInputValueType() : base("__InputValue", string.Empty)
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("defaultValue", e => null as string);
            this.Field("type", e => e.Type);
        }
    }
}