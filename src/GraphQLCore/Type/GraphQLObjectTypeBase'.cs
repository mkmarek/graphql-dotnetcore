namespace GraphQLCore.Type
{
    using Exceptions;
    using Execution;
    using Introspection;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public abstract class GraphQLObjectTypeBase<T> : GraphQLObjectType
    {
        private Dictionary<string, LambdaExpression> Acessors;

        public GraphQLObjectTypeBase(string name, string description, GraphQLSchema schema) : base(name, description, schema)
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

        public override IEnumerable<Type> GetFieldTypes()
        {
            return base.GetFieldTypes()
                .Union(this.Acessors.Select(e => ReflectionUtilities.GetReturnValueFromLambdaExpression(e.Value)))
                .ToList();
        }

        internal override object ResolveField(
            GraphQLFieldSelection selection, IList<GraphQLArgument> arguments, object parent)
        {
            var fieldName = this.GetFieldName(selection);

            if (base.ContainsField(fieldName))
                return base.ResolveField(selection, arguments, parent);

            return this.Acessors[fieldName].Compile().DynamicInvoke(parent);
        }

        protected void AddAcessor(string fieldName, LambdaExpression accessor)
        {
            if (this.ContainsField(fieldName))
                throw new GraphQLException("Can't insert two fields with the same name.");

            this.Acessors.Add(fieldName, accessor);
        }

        internal override __Field[] IntrospectFields()
        {
            var fields = base.IntrospectFields().ToList();

            fields.AddRange(this.Acessors
                .Select(e => new __Field(
                    e.Key,
                    null,
                    e.Value, this.schema, true)));

           return fields.ToArray();
        }
    }
}