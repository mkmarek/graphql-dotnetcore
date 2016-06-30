namespace GraphQLCore.Type
{
    using Exceptions;
    using Introspection;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectType : GraphQLScalarType
    {
        private Dictionary<string, LambdaExpression> Resolvers;

        public GraphQLObjectType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            this.Resolvers = new Dictionary<string, LambdaExpression>();
        }

        public virtual bool ContainsField(string fieldName)
        {
            return this.Resolvers.ContainsKey(fieldName);
        }

        public virtual IEnumerable<Type> GetFieldTypes()
        {
            return this.Resolvers
                .Select(e => ReflectionUtilities.GetReturnValueFromLambdaExpression(e.Value))
                .ToList();
        }

        internal void Field<TFieldType>(
                            string fieldName, LambdaExpression resolver)
        {
            this.AddResolver(fieldName, resolver);
        }

        internal void FieldIfNotExists<TFieldType>(
                            string fieldName, LambdaExpression resolver)
        {
            if (!this.ContainsField(fieldName))
                this.AddResolver(fieldName, resolver);
        }

        internal virtual __Field[] IntrospectFields()
        {
            return this.Resolvers
                .Select(e => new __Field(
                    e.Key,
                    null,
                    e.Value, this.schema, false))
                .ToArray();
        }

        internal virtual object ResolveField(
            GraphQLFieldSelection field, IList<GraphQLArgument> arguments, object parent)
        {
            var name = this.GetFieldName(field);

            if (this.Resolvers.ContainsKey(name))
            {
                var resolver = this.Resolvers[this.GetFieldName(field)];
                return TypeUtilities.InvokeWithArguments(arguments, resolver);
            }

            return null;
        }

        internal virtual object ResolveField(string name)
        {
            var resolver = this.Resolvers[name];
            return resolver.Compile().DynamicInvoke();
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
    }
}