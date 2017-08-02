namespace GraphQLCore.WsMiddleware.Handlers
{
    using Newtonsoft.Json;
    using Payloads;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using Type;

    public class GraphQLStartHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, OperationManager manager, OperationMessage input)
        {
            await Subscribe(socket, manager, input);
        }

        private static async Task Subscribe(WebSocket socket, OperationManager manager, OperationMessage input)
        {
            var payload = ((JObject)input.Payload).ToObject<StartPayload>();

            var observer = new ExecutionObserver(socket, input);
            var xxx = manager.Subscribe(input.Id, payload, observer);
        }
    }
}
