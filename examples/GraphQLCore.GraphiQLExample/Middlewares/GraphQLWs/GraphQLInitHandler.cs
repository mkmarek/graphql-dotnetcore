using GraphQLCore.Type;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQLCore.GraphiQLExample.Middlewares.GraphQLWs
{
    public class GraphQLInitHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            var dataString = JsonConvert.SerializeObject( new
            {
                type = "init_success"
            });

            var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

            await socket.SendAsync(
                new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
