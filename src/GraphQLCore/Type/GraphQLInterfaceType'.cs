namespace GraphQLCore.Type
{
    using System;
    using System.Linq.Expressions;

    public abstract class GraphQLInterfaceType<T> : GraphQLInterfaceType
        where T : class
    {
        public GraphQLInterfaceType(string name, string description, GraphQLSchema schema) : base(name, description, typeof(T), schema)
        {
        }

        public void Field<TProperty>(string name, Expression<Func<T, TProperty>> accessor)
        {
            this.fields.Add(name, accessor);
        }
    }
}