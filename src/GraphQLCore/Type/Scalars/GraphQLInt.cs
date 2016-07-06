namespace GraphQLCore.Type.Scalars
{
    public class GraphQLInt : GraphQLNullableType
    {
        public GraphQLInt() : base(
            "Int",
            "The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.")
        {
        }
    }
}