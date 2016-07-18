namespace GraphQLCore.Type
{
    using Language.AST;
    using Translation;
    public abstract class GraphQLInputType : GraphQLBaseType
    {
        public GraphQLInputType(string name, string description) : base(name, description)
        {
        }

        public abstract object GetFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository);
    }
}