namespace GraphQLCore.Type
{
    using Introspection;
    using Translation;

    public interface IGraphQLSchema
    {
        GraphQLObjectType MutationType { get; }
        GraphQLObjectType QueryType { get; }
        IntrospectedSchemaType IntrospectedSchema { get; }
        ISchemaRepository SchemaRepository { get; }

        dynamic Execute(string expression);

        dynamic Execute(string query, dynamic variables);

        dynamic Execute(string query, dynamic variables, string operationToExecute);

        void Query(GraphQLObjectType root);

        IntrospectedType IntrospectType(string name);
    }
}