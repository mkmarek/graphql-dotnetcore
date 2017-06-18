using GraphQLCore.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace GraphQLCore.GraphiQLExample.Middlewares.GraphQLWs
{
    public interface IGraphQLWsHandler
    {
        Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input);
    }
}
