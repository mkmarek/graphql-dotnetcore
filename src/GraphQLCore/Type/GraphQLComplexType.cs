namespace GraphQLCore.Type
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLComplexType : GraphQLNullableType
    {
        protected Dictionary<string, GraphQLObjectTypeFieldInfo> Fields { get; set; }

        public GraphQLComplexType(string name, string description) : base(name, description)
        {
            this.Fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
        }

        public bool ContainsField(string fieldName)
        {
            return this.Fields.ContainsKey(fieldName);
        }

        public GraphQLObjectTypeFieldInfo GetFieldInfo(string fieldName)
        {
            if (!this.ContainsField(fieldName))
                return null;

            return this.Fields[fieldName];
        }

        public GraphQLObjectTypeFieldInfo[] GetFieldsInfo()
        {
            return this.Fields.Select(e => e.Value)
                .ToArray();
        }

        protected GraphQLObjectTypeFieldInfo CreateFieldInfo<T, TProperty>(string fieldName, Expression<Func<T, TProperty>> accessor)
        {
            return new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = false,
                Lambda = accessor,
                Arguments = new Dictionary<string, GraphQLObjectTypeArgumentInfo>(),
                ReturnValueType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor)
            };
        }
    }
}