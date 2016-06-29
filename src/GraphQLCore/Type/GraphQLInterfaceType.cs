namespace GraphQLCore.Type
{
    using Exceptions;
    using System.Reflection;

    public abstract class GraphQLInterfaceType<T> : GraphQLObjectTypeBase<T>
        where T : class
    {
        public GraphQLInterfaceType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            if (!typeof(T).GetTypeInfo().IsInterface)
                throw new GraphQLException($" Type {name} has to be an interface type");
        }
    }
}