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
    public class GraphQLSubscriptionEndHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            await Task.Yield();

            if (input.Id.HasValue)
            {
                schema.Unsubscribe(clientId, input.Id.Value);
            }
        }
    }
}
