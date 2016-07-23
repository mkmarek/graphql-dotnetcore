namespace GraphQLCore.Type
{
    using Exceptions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType : GraphQLComplexType
    {
        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
        }

        public void Field<TFieldType>(string fieldName, LambdaExpression fieldLambda)
        {
            this.AddField(fieldName, fieldLambda);
        }

        protected virtual void AddField(string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Fields.Add(fieldName, this.CreateResolverFieldInfo(fieldName, resolver));
        }

        private GraphQLObjectTypeFieldInfo CreateResolverFieldInfo(string fieldName, LambdaExpression resolver)
        {
            return GraphQLObjectTypeFieldInfo.CreateResolverFieldInfo(fieldName, resolver);
        }
    }
}