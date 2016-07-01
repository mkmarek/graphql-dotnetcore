namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class GraphQLInterfaceType<T> : GraphQLInterfaceType
        where T : class
    {
        public GraphQLInterfaceType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            if (!typeof(T).GetTypeInfo().IsInterface)
                throw new GraphQLException($" Type {name} has to be an interface type");
        }

        public void Field<TProperty>(string name, Expression<Func<T, TProperty>> accessor)
        {
            this.fields.Add(name, accessor);
        }
    }
}