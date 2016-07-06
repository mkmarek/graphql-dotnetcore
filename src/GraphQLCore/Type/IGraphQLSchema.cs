namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using Translation;

    public interface IGraphQLSchema
    {
        IntrospectedSchemaType IntrospectedSchema { get; }
        GraphQLObjectType QueryType { get; }
        ITypeTranslator TypeTranslator { get; }

        dynamic Execute(string expression);

        void Query(GraphQLObjectType root);
    }
}