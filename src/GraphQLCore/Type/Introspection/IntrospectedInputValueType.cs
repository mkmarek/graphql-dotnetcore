namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedInputValueType : GraphQLObjectType<IntrospectedInputValue>
    {
        public IntrospectedInputValueType() : base(
            "__InputValue",
            "Arguments provided to Fields or Directives and the input fields of an " +
            "InputObject are represented as Input Values which describe their " +
            "type and optionally a default value.")
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("type", e => e.Type);
            this.Field("defaultValue", e => e.DefaultValue);
        }
    }
}