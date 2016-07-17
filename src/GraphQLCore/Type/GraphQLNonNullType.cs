namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using Language.AST;
    using Translation;

    public class GraphQLNonNullType : GraphQLInputType
    {
        public GraphQLNonNullType(GraphQLBaseType nullableType) : base(null, null)
        {
            this.UnderlyingNullableType = nullableType;
        }

        public GraphQLBaseType UnderlyingNullableType { get; private set; }

        public override object GetFromAst(GraphQLValue astValue)
        {
            if (this.UnderlyingNullableType is GraphQLInputType)
                return ((GraphQLInputType)this.UnderlyingNullableType).GetFromAst(astValue);

            return null;
        }

        public override IntrospectedType Introspect(ISchemaObserver schemaObserver)
        {
            var introspectedType = new IntrospectedType();
            introspectedType.Kind = TypeKind.NON_NULL;
            introspectedType.OfType = this.UnderlyingNullableType.Introspect(schemaObserver);

            return introspectedType;
        }
    }
}