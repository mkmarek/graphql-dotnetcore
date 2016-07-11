namespace GraphQLCore.Type
{
    public class IntrospectedInputObjectType : GraphQLObjectType<IntrospectedInputObject>
    {
        public IntrospectedInputObjectType() : base(null, null)
        {
            this.Field("kind", e => e.Kind);
            this.Field("fields", e => e.Fields);
        }
    }
}