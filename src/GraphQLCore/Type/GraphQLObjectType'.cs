namespace GraphQLCore.Type
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType<T> : GraphQLObjectType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
        }

        public void Field<TFieldType>(
            string fieldName, Expression<Func<T, TFieldType>> accessor)
        {
            this.AddAcessor(fieldName, accessor);
        }

        public void Field<TFieldType>(Expression<Func<T, TFieldType>> accessor)
        {
            this.Field(ReflectionUtilities.GetPropertyInfo(accessor).Name, accessor);
        }

        internal override object ResolveField(
            GraphQLFieldSelection field, IList<GraphQLArgument> arguments, object parent)
        {
            var name = this.GetFieldName(field);

            if (this.fields.ContainsKey(name))
            {
                var fieldInfo = this.fields[this.GetFieldName(field)];

                if (fieldInfo.IsResolver)
                    return base.ResolveField(field, arguments, parent);

                return this.ProcessField(fieldInfo.Lambda.Compile().DynamicInvoke(new object[] { parent }));
            }

            return null;
        }

        protected void AddAcessor(string fieldName, LambdaExpression accessor)
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