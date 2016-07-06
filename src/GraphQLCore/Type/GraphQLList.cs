namespace GraphQLCore.Type
{
    public class GraphQLList : GraphQLNullableType
    {
        public GraphQLList(GraphQLScalarType memberType) : base(null, null)
        {
            this.MemberType = memberType;
        }

        public GraphQLScalarType MemberType { get; private set; }
    }
}