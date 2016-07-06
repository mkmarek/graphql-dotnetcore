namespace GraphQLCore.Type
{
    public class GraphQLNonNullType : GraphQLNullableType
    {
        public GraphQLNonNullType(GraphQLNullableType nullableType) : base(null, null)
        {
            this.UnderlyingNullableType = nullableType;
        }

        public GraphQLNullableType UnderlyingNullableType { get; private set; }
    }
}