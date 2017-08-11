namespace GraphQLCore.Type
{
    using Complex;
    using Introspection;
    using System;
    using System.Threading.Tasks;
    using Events;
    using Execution;
    using Translation;

    public interface IGraphQLSchema
    {
        event EventHandler<OnMessageReceivedEventArgs> OnSubscriptionMessageReceived;
        GraphQLObjectType MutationType { get; }
        GraphQLObjectType QueryType { get; }
        GraphQLSubscriptionType SubscriptionType { get; }
        IntrospectedSchemaType IntrospectedSchema { get; }
        ISchemaRepository SchemaRepository { get; }

        ExecutionResult Execute(string expression);

        ExecutionResult Execute(string query, dynamic variables);

        ExecutionResult Execute(string query, dynamic variables, string operationToExecute);

        ExecutionResult Execute(string query, dynamic variables, string operationToExecute, string clientId, string subscriptionId);

        Task<ExecutionResult> ExecuteAsync(string expression);

        Task<ExecutionResult> ExecuteAsync(string query, dynamic variables);

        Task<ExecutionResult> ExecuteAsync(string query, dynamic variables, string operationToExecute);

        Task<ExecutionResult> ExecuteAsync(string query, dynamic variables, string operationToExecute, string clientId, string subscriptionId);

        IObservable<ExecutionResult> Subscribe(string query);

        IObservable<ExecutionResult> Subscribe(string query, dynamic variables);

        IObservable<ExecutionResult> Subscribe(string query, dynamic variables, string operationToExecute);

        IObservable<ExecutionResult> Subscribe(string query, dynamic variables, string operationToExecute, string clientId, string subscriptionId);

        void Unsubscribe(string clientId, string subscriptionId);

        void Query(GraphQLObjectType root);

        IntrospectedType IntrospectType(string name);
        void Unsubscribe(string clientId);
    }
}