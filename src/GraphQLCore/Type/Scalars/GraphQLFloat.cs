namespace GraphQLCore.Type.Scalars
{
    public class GraphQLFloat : GraphQLNullableType
    {
        public GraphQLFloat() : base(
            "Float",
            "The `Float` scalar type represents signed double-precision fractional " +
            "values as specified by " +
            "[IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point). ")
        {
        }
    }
}