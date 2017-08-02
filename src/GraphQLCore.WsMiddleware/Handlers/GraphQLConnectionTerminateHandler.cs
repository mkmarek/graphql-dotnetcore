namespace GraphQLCore.WsMiddleware.Handlers
{
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Type;

    public class GraphQLConnectionTerminateHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, OperationManager manager, OperationMessage input)
        {
            await Task.Yield();

            manager.Dispose();

            await socket.CloseAsync(socket.CloseStatus.GetValueOrDefault(), socket.CloseStatusDescription, CancellationToken.None);
        }
    }
}
