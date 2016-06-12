using System;
using System.Linq.Expressions;

namespace GraphQL.Type
{
    public class GraphQLObjectType<T> : GraphQLScalarType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
        }

        public void AddField<TFieldType>(string fieldName, Expression<Func<T, object>> valueAccessor)
        {
        }
    }
}
