namespace GraphQL.Type
{
    using System;
    using System.Linq.Expressions;
    using Language.AST;
    using Exceptions;
    using System.Collections.Generic;
    using Utils;
    using System.Linq;
    using Execution;
    public class GraphQLObjectType<T> : GraphQLObjectType
        where T : class
    {
        private LambdaExpression Resolver;
        private Dictionary<string, LambdaExpression> Acessors;

        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Acessors = new Dictionary<string, LambdaExpression>();
        }

        public override bool ContainsField(string fieldName)
        {
            return this.Acessors.ContainsKey(fieldName) || base.ContainsField(fieldName);
        }

        public void Field<TFieldType>(
            string fieldName, Expression<Func<T, TFieldType>> accessor)
        {
            this.AddAcessor(fieldName, accessor);
        }

        public void Field<TFieldType>(Expression<Func<T, TFieldType>> accessor)
        {
            this.Field(ReflectionUtilities.GetPropertyInfo(accessor).Name, accessor);
        }

        protected void AddAcessor(string fieldName, LambdaExpression accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Acessors.Add(fieldName, accessor);
        }

        internal override object ResolveField(
            GraphQLFieldSelection selection, Dictionary<int, object> ResolvedObjectCache, IList<GraphQLArgument> arguments)
        {
            if (base.ContainsField(this.GetFieldName(selection)))
                return base.ResolveField(selection, ResolvedObjectCache, arguments);

            var instance = this.ResolveInstance(selection, ResolvedObjectCache, arguments);

            return this.Acessors[this.GetFieldName(selection)].Compile().DynamicInvoke(instance);
        }

        private object ResolveInstance(
            GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache, IList<GraphQLArgument> arguments)
        {
            return ResolvedObjectCache.ContainsKey(field.Location.Start)
                ? ResolvedObjectCache[field.Location.Start]
                : ExecuteResolver(field, ResolvedObjectCache, arguments);
        }

        private object ExecuteResolver(GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache, IList<GraphQLArgument> arguments)
        {
            if (this.Resolver == null)
                throw new GraphQLException($"GraphQLObjectType {this.Name} doesn't have a resolver");

            var instance = this.Resolver.Compile().DynamicInvoke(this.FetchArgumentValues(this.Resolver, arguments));
            ResolvedObjectCache.Add(field.Location.Start, instance);

            return instance;
        }

        internal void SetResolver(LambdaExpression resolver)
        {
            this.Resolver = resolver;
        }
    }
}
