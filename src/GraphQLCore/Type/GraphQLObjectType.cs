namespace GraphQLCore.Type
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Language.AST;
    using Exceptions;
    using Utils;
    using System.Linq;
    using Execution;
    using System;
    using Introspection;
    public class GraphQLObjectType : GraphQLScalarType
    {
        private Dictionary<string, LambdaExpression> Resolvers;
        private IList<GraphQLInterface> Interfaces;

        public GraphQLObjectType(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            this.Resolvers = new Dictionary<string, LambdaExpression>();
            this.Interfaces = new List<GraphQLInterface>();
        }

        internal void Field<TFieldType>(
            string fieldName, LambdaExpression resolver)
        {
            this.AddResolver(fieldName, resolver);
        }

        public virtual bool ContainsField(string fieldName)
        {
            return this.Resolvers.ContainsKey(fieldName);
        }

        internal __Field[] IntrospectFields()
        {
            return this.Resolvers
                .Select(e => new __Field(
                    e.Key, 
                    null, 
                    ReflectionUtilities.GetReturnValueFromLambdaExpression(e.Value)))
                .ToArray();
        }

        protected object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => this.ChangeValueType(ExecutionContext.GetArgumentValue(arguments, e.Name), e))
                .ToArray();
        }

        protected object ChangeValueType(object input, ParameterExpression parameter)
        {
            if (input is IEnumerable<object> && ReflectionUtilities.IsCollection(parameter.Type))
                return ReflectionUtilities.ChangeToCollection(input, parameter);

            return TryConvertToParameterType(input, parameter);
        }

        private static object TryConvertToParameterType(object input, ParameterExpression parameter)
        {
            try
            {
                return Convert.ChangeType(input, parameter.Type);
            }
            catch (Exception ex)
            {
                throw new GraphQLException($"Can't convert input of type {input.GetType().Name} to {parameter.Type.Name}.", ex);
            }
        }

        public virtual IEnumerable<Type> GetFieldTypes()
        {
            return this.Resolvers
                .Select(e => ReflectionUtilities.GetReturnValueFromLambdaExpression(e.Value))
                .ToList();
        }

        protected void AddResolver(string fieldName, LambdaExpression resolver)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Resolvers.Add(fieldName, resolver);
        }

        public void Implements(GraphQLInterface nestedInteface)
        {
            this.Interfaces.Add(nestedInteface);
        }

        protected string GetFieldName(GraphQLFieldSelection selection)
        {
            return selection.Name?.Value ?? selection.Alias?.Value;
        }

        internal virtual object ResolveField(
            GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache, IList<GraphQLArgument> arguments)
        {
            var name = this.GetFieldName(field);

            if (this.Resolvers.ContainsKey(name))
            {
                var resolver = this.Resolvers[this.GetFieldName(field)];
                var argumentValues = this.FetchArgumentValues(resolver, arguments);

                return resolver.Compile().DynamicInvoke(argumentValues);
            }

            return null;
        }

        internal virtual object ResolveField(string name)
        {
            var resolver = this.Resolvers[name];
            return resolver.Compile().DynamicInvoke();
        }
    }
}
