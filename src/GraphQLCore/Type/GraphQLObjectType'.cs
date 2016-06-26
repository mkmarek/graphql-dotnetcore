namespace GraphQLCore.Type
{
    public class GraphQLObjectType<T> : GraphQLObjectTypeBase<T>
        where T : class, new()
    {
        public GraphQLObjectType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
        }
    }
}