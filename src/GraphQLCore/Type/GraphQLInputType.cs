namespace GraphQLCore.Type
{
    using Language.AST;
    using Translation;
    using Utils;

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

        public abstract object GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository);

        public object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
        {
            if (astValue.Kind == ASTNodeKind.Variable)
            {
                var value = schemaRepository.VariableResolver.GetValue((GraphQLVariable)astValue);
                value = ReflectionUtilities.ChangeValueType(value, schemaRepository.GetInputSystemTypeFor(this));

                return value;
            }

            return this.GetValueFromAst(astValue, schemaRepository);
        }
    }
}
