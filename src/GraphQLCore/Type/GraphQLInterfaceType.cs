namespace GraphQLCore.Type
{
    using System;
    using System.Collections.Generic;
    using Introspection;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Exceptions;

    public abstract class GraphQLInterfaceType : GraphQLScalarType
    {
        protected Dictionary<string, LambdaExpression> fields = new Dictionary<string, LambdaExpression>();
        private Type interfaceType;

        public GraphQLInterfaceType(string name, string description, Type interfaceType, GraphQLSchema schema) : base(name, description, schema)
        {
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new GraphQLException($" Type {name} has to be an interface type");

            this.interfaceType = interfaceType;
        }

        internal Type GetInterfaceType()
        {
            return this.interfaceType;
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