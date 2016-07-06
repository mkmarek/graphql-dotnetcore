namespace GraphQLCore.Type
{
    public class GraphQLEnumType<T> : GraphQLEnumType
    {
        public GraphQLEnumType(string name, string description) :
            base(name, description, typeof(T))
        { }
    }
}