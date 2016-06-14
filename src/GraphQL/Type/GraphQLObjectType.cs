using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using GraphQL.Language.AST;

namespace GraphQL.Type
{
    public class GraphQLObjectType : GraphQLScalarType
    {
        private Dictionary<string, LambdaExpression> Resolvers;

        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Resolvers = new Dictionary<string, LambdaExpression>();
        }

        public void AddResolver(string fieldName, LambdaExpression resolver)
        {
            this.Resolvers.Add(fieldName, resolver);
        }

        internal object ResolveField(GraphQLFieldSelection field)
        {
            var resolver = this.Resolvers[field.Name.Value];

            return resolver.Compile().DynamicInvoke();
        }
    }
}
