namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using Language.AST;
    using Translation;

    public class GraphQLNonNull : GraphQLInputType
    {
        public override bool IsLeafType
        {
            get
            {
                return this.UnderlyingNullableType.IsLeafType;
            }
        }

        public GraphQLNonNull(GraphQLBaseType nullableType) : base(null, null)
        {
            this.UnderlyingNullableType = nullableType;

            if (nullableType != null)
                this.Name = nullableType.ToString();
        }

        public GraphQLBaseType UnderlyingNullableType { get; private set; }

        public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (this.UnderlyingNullableType is GraphQLInputType)
            {
                var result = ((GraphQLInputType)this.UnderlyingNullableType).GetFromAst(astValue, schemaRepository);

                return result.Value != null ? result : Result.Invalid;
            }

            return Result.Invalid;
        }

        public override NonNullable<IntrospectedType> Introspect(ISchemaRepository schemaRepository)
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

        protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
        {
            if (this.UnderlyingNullableType is GraphQLInputType)
            {
                var astValue = ((GraphQLInputType)this.UnderlyingNullableType).GetAstFromValue(value, schemaRepository);

                if (astValue.Kind == ASTNodeKind.NullValue)
                    return null;

                return astValue;
            }

            return null;
        }
    }
}