namespace GraphQLCore.GraphiQLExample.Middlewares.GraphQLWs
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using Newtonsoft.Json;
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class GraphQLInitHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            var dataString = schema == null
                ? CreateFailDataString(new GraphQLException("The server schema is invalid or does not exist."))
                : CreateSuccessDataString();

            var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

            await socket.SendAsync(
                new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static string CreateSuccessDataString()
        {
            return JsonConvert.SerializeObject(new
            {
                type = "init_success"
            });
        }

        private static string CreateFailDataString(Exception error)
        {
            return JsonConvert.SerializeObject(new
            {
                type = "init_fail",
                error
            });
        }
    }
}
