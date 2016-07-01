namespace GraphQLCore.Type
{
    public abstract class GraphQLObjectType<T> : GraphQLObjectTypeBase<T>
        where T : new()
    {
        public GraphQLObjectType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
        }
    }
}