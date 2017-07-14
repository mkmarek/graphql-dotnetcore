namespace GraphQLCore.Type
{
    using Language.AST;
    using Translation;

    public abstract class GraphQLInputType : GraphQLBaseType
    {
        public override bool IsLeafType
        {
            get
            {
                return false;
            }
        }

        public GraphQLInputType(string name, string description) : base(name, description)
        {
        }

        public abstract Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository);

        public Result GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (!(this is GraphQLNonNull) && (astValue == null || astValue.Kind == ASTNodeKind.NullValue))
                return new Result(null);

            if (astValue?.Kind == ASTNodeKind.Variable)
            {
                var result = schemaRepository.VariableResolver.GetValue((GraphQLVariable)astValue);

                if (result.Value == null && this is GraphQLNonNull)
                    return Result.Invalid;

                return result;
            }

            return this.GetValueFromAst(astValue, schemaRepository);
        }

        public GraphQLValue GetAstFromValue(object value, ISchemaRepository schemaRepository)
        {
            if (value == null && !(this is GraphQLNonNull))
                return new GraphQLNullValue();

            return this.GetAst(value, schemaRepository);
        }

        protected abstract GraphQLValue GetAst(object value, ISchemaRepository schemaRepository);
    }
}
