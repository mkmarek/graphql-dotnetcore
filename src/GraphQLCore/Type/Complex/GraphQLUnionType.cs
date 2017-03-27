namespace GraphQLCore.Type
{
    using Exceptions;
    using System;
    using System.Reflection;
    using System.Linq.Expressions;
    using Utils;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQLCore.Type.Introspection;
    using GraphQLCore.Type.Translation;

    public abstract class GraphQLUnionType : GraphQLComplexType
    {
        public override Type SystemType { get; protected set; }

        private List<Type> possibleTypes;
        public IEnumerable<Type> PossibleTypes { get { return this.possibleTypes; } }

        public GraphQLUnionType(string name, string description) : base(name, description)
        {
            this.SystemType = this.GetType();
            this.possibleTypes = new List<Type>();
        }

        protected void AddPossibleType(Type type) { this.possibleTypes.Add(type); }

        public abstract Type ResolveType(object data);

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = base.Introspect(schemaRepository);

            introspectedType.Kind = TypeKind.UNION;

            return introspectedType;
        }
    }
}