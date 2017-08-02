namespace GraphQLCore.WsMiddleware.Handlers
{
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Type;

    public class GraphQLStopHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, OperationManager manager, OperationMessage input)
        {
            await Task.Yield();

            if (input.Id != null)
            {
                manager.Unsubscribe(input.Id);
                await socket.SendResponse(MessageType.GQL_COMPLETE, input.Id);
            }
        }
    }
}
