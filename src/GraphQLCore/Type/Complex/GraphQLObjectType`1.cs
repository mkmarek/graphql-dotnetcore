namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType<T> : GraphQLObjectType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.SystemType = typeof(T);
        }

        public void Field<TFieldType>(string fieldName, Expression<Func<T, TFieldType>> accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Fields.Add(fieldName, this.CreateFieldInfo(fieldName, accessor));
        }

        public void Field<TFieldType>(Expression<Func<T, TFieldType>> accessor)
        {
            this.Field(ReflectionUtilities.GetPropertyInfo(accessor).Name, accessor);
        }
    }
}