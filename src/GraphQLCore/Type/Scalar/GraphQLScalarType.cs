namespace GraphQLCore.Type.Scalar
{
    using Introspection;
    using Translation;

    public abstract class GraphQLScalarType : GraphQLInputType
    {
        public GraphQLScalarType(string name, string description) : base(name, description)
        {
        }

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Name = this.Name;
            introspectedType.Description = this.Description;
            introspectedType.Kind = TypeKind.SCALAR;

            return introspectedType;
        }
    }
}