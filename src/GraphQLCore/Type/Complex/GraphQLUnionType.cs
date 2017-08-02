namespace GraphQLCore.Type
{
    using Introspection;
    using System;
    using System.Collections.Generic;
    using Translation;

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

        public abstract Type ResolveType(object data);

        public override NonNullable<IntrospectedType> Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = base.Introspect(schemaRepository);

            introspectedType.Value.Kind = TypeKind.UNION;

            return introspectedType;
        }

        protected void AddPossibleType(Type type) { this.possibleTypes.Add(type); }
    }
}