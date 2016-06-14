using System;
using System.Linq.Expressions;

namespace GraphQL.Type
{
    public class GraphQLObjectType<T> : GraphQLObjectType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
        }

        public void AddField<TFieldType>(
            string fieldName, Expression<Func<TFieldType>> resolver)
        {
            base.AddResolver(fieldName, resolver);
        }
    }
}
