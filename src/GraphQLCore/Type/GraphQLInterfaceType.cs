namespace GraphQLCore.Type
{
    using System;
    using System.Collections.Generic;
    using Introspection;
    using System.Linq;
    using System.Linq.Expressions;
    public abstract class GraphQLInterfaceType : GraphQLScalarType
    {
        protected Dictionary<string, LambdaExpression> fields = new Dictionary<string, LambdaExpression>();

        public GraphQLInterfaceType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
        }

        internal __Field[] IntrospectFields()
        {
            return this.fields
                .Select(e => new __Field(
                    e.Key,
                    null,
                    e.Value, this.schema, true))
                .ToArray();
        }
    }
}