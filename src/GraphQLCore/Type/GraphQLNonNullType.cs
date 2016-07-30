namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using Language.AST;
    using Translation;

    public class GraphQLNonNullType : GraphQLInputType
    {
        public override bool IsLeafType
        {
            get
            {
                return this.UnderlyingNullableType.IsLeafType;
            }
        }

        public GraphQLNonNullType(GraphQLBaseType nullableType) : base(null, null)
        {
            this.UnderlyingNullableType = nullableType;
        }

        public GraphQLBaseType UnderlyingNullableType { get; private set; }

        public override object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (this.UnderlyingNullableType is GraphQLInputType)
                return ((GraphQLInputType)this.UnderlyingNullableType).GetFromAst(astValue, schemaRepository);

            return null;
        }

        public override IntrospectedType Introspect(ISchemaRepository schemaRepository)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Kind = TypeKind.NON_NULL;
            introspectedType.OfType = this.UnderlyingNullableType.Introspect(schemaRepository);

            return introspectedType;
        }

        public override string ToString()
        {
            return this.UnderlyingNullableType.ToString() + "!";
        }
    }
}