namespace GraphQLCore.Type.Introspection
{
    public class IntrospectedDirectiveType : GraphQLObjectType<IntrospectedDirective>
    {
        public IntrospectedDirectiveType() : base(
            "__Directive",
            string.Empty)
        {
            this.Field("name", e => e.Name);
            this.Field("description", e => e.Description);
            this.Field("locations", e => e.Locations);
            this.Field("args", e => e.GetArgs());
        }
    }
}