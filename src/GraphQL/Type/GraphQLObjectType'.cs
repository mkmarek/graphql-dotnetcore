namespace GraphQL.Type
{
    using System;
    using System.Linq.Expressions;
    using Language.AST;
    using Exceptions;
    using System.Collections.Generic;

    public class GraphQLObjectType<T> : GraphQLObjectType
        where T : class
    {
        private Func<T> Resolver;
        private Dictionary<string, LambdaExpression> Acessors;

        public GraphQLObjectType(string name, string description) : base(name, description)
        {
            this.Acessors = new Dictionary<string, LambdaExpression>();
        }

        public override bool ContainsField(string fieldName)
        {
            return this.Acessors.ContainsKey(fieldName) || base.ContainsField(fieldName);
        }

        public void AddField<TFieldType>(
            string fieldName, Expression<Func<T, TFieldType>> accessor)
        {
            this.AddAcessor(fieldName, accessor);
        }

        protected void AddAcessor(string fieldName, LambdaExpression accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Acessors.Add(fieldName, accessor);
        }

        internal override object ResolveField(GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache)
        {
            if (base.ContainsField(this.GetFieldName(field)))
                return base.ResolveField(field, ResolvedObjectCache);

            var instance = this.ResolveInstance(field, ResolvedObjectCache);

            return this.Acessors[this.GetFieldName(field)].Compile().DynamicInvoke(instance);
        }

        private object ResolveInstance(GraphQLFieldSelection field, Dictionary<int, object> ResolvedObjectCache)
        {
            object instance;

            if (!ResolvedObjectCache.ContainsKey(field.Location.Start))
            {
                if (this.Resolver == null)
                    throw new GraphQLException($"GraphQLObjectType {this.Name} doesn't have a resolver");

                instance = this.Resolver.DynamicInvoke();
                ResolvedObjectCache.Add(field.Location.Start, instance);
            }
            else
            {
                instance = ResolvedObjectCache[field.Location.Start];
            }

            return instance;
        }

        public void SetResolver(Func<T> resolver)
        {
            this.Resolver = resolver;
        }
    }
}
