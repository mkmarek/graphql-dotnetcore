namespace GraphQLCore.Type
{
    using Complex;
    using Exceptions;
    using Execution;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class GraphQLObjectType : GraphQLComplexType
    {
        public override System.Type SystemType { get; protected set; }

        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
            this.SystemType = this.GetType();
        }

        public FieldDefinitionBuilder Field(string fieldName, LambdaExpression fieldLambda, string description)
        {
            return this.AddField(fieldName, fieldLambda, description);
        }

        protected virtual FieldDefinitionBuilder AddField(string fieldName, LambdaExpression resolver, string description)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.ValidateResolver(resolver);

            var fieldInfo = this.CreateResolverFieldInfo(fieldName, resolver, description);

            this.Fields.Add(fieldName, fieldInfo);

            return new FieldDefinitionBuilder(fieldInfo);
        }

        private GraphQLObjectTypeFieldInfo CreateResolverFieldInfo(string fieldName, LambdaExpression resolver, string description)
        {
            return GraphQLObjectTypeFieldInfo.CreateResolverFieldInfo(fieldName, resolver, description);
        }
    }
}