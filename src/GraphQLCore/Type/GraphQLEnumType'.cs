namespace GraphQLCore.Type
{
    public class GraphQLEnumType<T> : GraphQLEnumType
    {
        public GraphQLEnumType(string name, string description, GraphQLSchema schema) : 
            base(name, description, typeof(T), schema) { }
    }
}