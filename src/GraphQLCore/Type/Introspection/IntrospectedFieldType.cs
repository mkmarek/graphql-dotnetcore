namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedFieldType : GraphQLObjectType<IntrospectedField>
    {
        public IntrospectedFieldType() : base(
            "__Field",
            "Object and Interface types are described by a list of Fields, each of " +
            "which has a name, potentially a list of arguments, and a return type.")
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("isDeprecated", e => e.IsDeprecated);
            this.Field("deprecationReason", e => e.DeprecationReason);
            this.Field("args", e => e.Arguments);
            this.Field("type", e => e.Type);
        }
    }
}