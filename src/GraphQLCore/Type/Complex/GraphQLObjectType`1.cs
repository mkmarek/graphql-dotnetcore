namespace GraphQLCore.Type
{
    using Exceptions;
    using GraphQLCore.Type.Complex;
    using System;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType<T> : GraphQLObjectType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.SystemType = typeof(T);
        }

        public FieldDefinitionBuilder Field<TFieldType>(string fieldName, Expression<Func<T, TFieldType>> accessor, string description = null)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            var fieldInfo = this.CreateFieldInfo(fieldName, accessor, description);

            this.Fields.Add(fieldName, fieldInfo);

            return new FieldDefinitionBuilder(fieldInfo);
        }

        public FieldDefinitionBuilder Field<TFieldType>(Expression<Func<T, TFieldType>> accessor, string description = null)
        {
            return this.Field(ReflectionUtilities.GetPropertyInfo(accessor).Name, accessor, description);
        }
    }
}