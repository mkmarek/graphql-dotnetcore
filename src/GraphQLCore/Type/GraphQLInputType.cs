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

        public abstract object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository);
    }
}