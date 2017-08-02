namespace GraphQLCore.WsMiddleware.Handlers
{
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using GraphQLCore.Type;

    public interface IGraphQLWsHandler
    {
        Task Handle(WebSocket socket, OperationManager manager, OperationMessage input);
    }
}
