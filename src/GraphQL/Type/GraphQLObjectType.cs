namespace GraphQL.Type
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Language.AST;
    using System.Linq;
    using Exceptions;
    using System;
    public class GraphQLObjectType : GraphQLScalarType
    {
        private Dictionary<string, LambdaExpression> Resolvers;


        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Resolvers = new Dictionary<string, LambdaExpression>();;
        }

        public void AddField<TFieldType>(
            string fieldName, Expression<Func<TFieldType>> resolver)
        {
            this.AddResolver(fieldName, resolver);
        }

        public virtual bool ContainsField(string fieldName)
        {
            return this.Resolvers.ContainsKey(fieldName);
        }

        protected void AddResolver(string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Resolvers.Add(fieldName, resolver);
        }

        protected string GetFieldName(GraphQLFieldSelection selection)
        {
            return selection.Name?.Value ?? selection.Alias?.Value;
        }

        internal virtual object ResolveField(GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache)
        {
            var resolver = this.Resolvers[this.GetFieldName(field)];

            return resolver.Compile().DynamicInvoke();
        }
    }
}
