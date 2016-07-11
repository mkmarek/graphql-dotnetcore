namespace GraphQLCore.Type
{
    using Introspection;
    using Translation;

    public interface IGraphQLSchema
    {
        IntrospectedSchemaType IntrospectedSchema { get; }
        GraphQLObjectType MutationType { get; }
        GraphQLObjectType QueryType { get; }
        ITypeTranslator TypeTranslator { get; }

        dynamic Execute(string expression);

        dynamic Execute(string query, dynamic variables);

        void Query(GraphQLObjectType root);
    }
}