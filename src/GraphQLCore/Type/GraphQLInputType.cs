namespace GraphQLCore.Type
{
    using GraphQLCore.Language.AST;

    public abstract class GraphQLInputType : GraphQLBaseType
    {
        public GraphQLInputType(string name, string description) : base(name, description)
        {
        }

        public abstract object GetFromAst(GraphQLValue astValue);
    }
}