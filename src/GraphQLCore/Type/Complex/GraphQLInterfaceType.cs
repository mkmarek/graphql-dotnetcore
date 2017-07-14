namespace GraphQLCore.Type
{
    using Exceptions;
    using Introspection;
    using System;
    using System.Reflection;
    using Translation;

    public abstract class GraphQLInterfaceType : GraphQLComplexType
    {
        private Type interfaceType;

        public GraphQLInterfaceType(string name, string description, Type interfaceType) : base(name, description)
        {
            if (!interfaceType.GetTypeInfo().IsInterface)
                throw new GraphQLException($"Type {interfaceType.FullName} has to be an interface type");

            this.interfaceType = interfaceType;
        }

        public override NonNullable<IntrospectedType> Introspect(ISchemaRepository schemaRepository)
        {
            var type = base.Introspect(schemaRepository);

            type.Value.Kind = TypeKind.INTERFACE;

            return type;
        }
    }
}