namespace GraphQLCore.Type
{
    public interface IGraphQLSchema
    {
        dynamic Execute(string expression);
        void Query(GraphQLObjectType root);
    }
}