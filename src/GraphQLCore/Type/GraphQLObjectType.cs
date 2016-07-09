namespace GraphQLCore.Type
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType : GraphQLComplexType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
        }

        public void Field<TFieldType>(string fieldName, LambdaExpression fieldLambda)
        {
            this.AddField(fieldName, fieldLambda);
        }

        protected virtual void AddField(string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.fields.Add(fieldName, new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = true,
                Lambda = resolver,
                Arguments = this.GetArguments(resolver),
                ReturnValueType = ReflectionUtilities.GetReturnValueFromLambdaExpression(resolver)
            });
        }

        private Dictionary<string, GraphQLObjectTypeArgumentInfo> GetArguments(LambdaExpression resolver)
        {
            return resolver.Parameters.Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                Type = e.Type
            }).ToDictionary(e => e.Name);
        }
    }
}