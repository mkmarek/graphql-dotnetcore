namespace GraphQLCore.WsMiddleware.Handlers
{
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using Payloads;

    public class GraphQLConnectionInitHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, OperationManager manager, OperationMessage input)
        {
            if (manager.Schema != null)
                await socket.SendResponse(MessageType.GQL_CONNECTION_ACK);
            else
                await socket.SendResponse(MessageType.GQL_CONNECTION_ERROR, null, new ErrorPayload()
                { 
                    Error = new GraphQLException("The server schema is invalid or does not exist.")
                });
        }
    }
}
