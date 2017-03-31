namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Complex;
    using Introspection;
    using Translation;

    public interface IGraphQLSchema
    {
        GraphQLObjectType MutationType { get; }
        GraphQLObjectType QueryType { get; }
        GraphQLSubscriptionType SubscriptionType { get; }
        IntrospectedSchemaType IntrospectedSchema { get; }
        ISchemaRepository SchemaRepository { get; }

        dynamic Execute(string expression);

        dynamic Execute(string query, dynamic variables);

        dynamic Execute(string query, dynamic variables, string operationToExecute);

        void Query(GraphQLObjectType root);

        IntrospectedType IntrospectType(string name);
    }
}