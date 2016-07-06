namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLInterfaceType<T> : GraphQLInterfaceType
        where T : class
    {
        public GraphQLInterfaceType(string name, string description) : base(name, description, typeof(T))
        {
        }

        public void Field<TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.fields.Add(fieldName, new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = false,
                Lambda = accessor,
                Arguments = new Dictionary<string, GraphQLObjectTypeArgumentInfo>(),
                ReturnValueType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor)
            });
        }
    }
}