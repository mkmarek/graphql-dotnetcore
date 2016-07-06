namespace GraphQLCore.Type.Scalars
{
    public class GraphQLBoolean : GraphQLNullableType
    {
        public GraphQLBoolean() : base(
            "Boolean",
            "The `Boolean` scalar type represents `true` or `false`.")
        {
        }
    }
}