namespace GraphQLCore.GraphiQLExample.Middlewares.GraphQLWs
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class GraphQLSubscriptionStartHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            await Subscribe(socket, clientId, schema, input);
        }

        private static async Task Subscribe(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            var data = (IDictionary<string, object>)schema.Execute(input.Query, null, null, clientId, input.Id.Value);

            if (data.ContainsKey("errors"))
                await SendResponseToExceptions(socket, input.Id.Value, data["errors"]);
            else
            {
                var dataString = JsonConvert.SerializeObject(new {id = input.Id, type = "subscription_success"});
                await SendResponse(socket, dataString);
            }
        }

        private static async Task SendResponseToExceptions(WebSocket socket, int id, object errors)
        {
            var dataString = JsonConvert.SerializeObject(new
            {
                id,
                type = "subscription_fail",
                payload = new
                {
                    errors
                }
            });

            await SendResponse(socket, dataString);
        }

        private static async Task SendResponse(WebSocket socket, string dataString)
        {
            var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

            await socket.SendAsync(
                new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
