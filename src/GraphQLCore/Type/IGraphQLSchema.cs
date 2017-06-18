namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Complex;
    using Introspection;
    using System.Threading.Tasks;
    using Translation;

    public interface IGraphQLSchema
    {
        event SubscriptionMessageReceived OnSubscriptionMessageReceived;
        GraphQLObjectType MutationType { get; }
        GraphQLObjectType QueryType { get; }
        GraphQLSubscriptionType SubscriptionType { get; }
        IntrospectedSchemaType IntrospectedSchema { get; }
        ISchemaRepository SchemaRepository { get; }

        dynamic Execute(string expression);

        dynamic Execute(string query, dynamic variables);

        dynamic Execute(string query, dynamic variables, string operationToExecute);

        dynamic Execute(string query, dynamic variables, string operationToExecute, string clientId, int subscriptionId);

        Task<dynamic> ExecuteAsync(string expression);

        Task<dynamic> ExecuteAsync(string query, dynamic variables);

        Task<dynamic> ExecuteAsync(string query, dynamic variables, string operationToExecute);

        Task<dynamic> ExecuteAsync(string query, dynamic variables, string operationToExecute, string clientId, int subscriptionId);

        void Unsubscribe(string clientId, int subscriptionId);

        void Query(GraphQLObjectType root);

        IntrospectedType IntrospectType(string name);
        void Unsubscribe(string clientId);
    }
}