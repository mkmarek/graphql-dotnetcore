namespace GraphQLCore.WsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQLCore.Type;
    using Handlers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Payloads;

    public static class GraphQLWsMiddleware
    {
        private static Dictionary<MessageType, IGraphQLWsHandler> handlers = new Dictionary<MessageType, IGraphQLWsHandler>()
        {
            { MessageType.GQL_CONNECTION_INIT, new GraphQLConnectionInitHandler() },
            { MessageType.GQL_CONNECTION_TERMINATE, new GraphQLConnectionTerminateHandler() },
            { MessageType.GQL_START, new GraphQLStartHandler() },
            { MessageType.GQL_STOP, new GraphQLStopHandler() }
        };

        public static void AddGraphQLWs(this IApplicationBuilder app)
        {
            app.Use(Middleware);
        }

        private static async Task Middleware(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path == "/graphql")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync("graphql-ws");

                    await StartCommunication(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        }

        private static async Task StartCommunication(HttpContext context, WebSocket webSocket)
        {
            var operationManager = new OperationManager(GetSchema(context));

            await MainLoop(webSocket, operationManager);
        }

        private static IGraphQLSchema GetSchema(HttpContext context)
        {
            try
            {
                return context.RequestServices.GetService(typeof(IGraphQLSchema)) as IGraphQLSchema;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static async Task<WebSocketReceiveResult> MainLoop(WebSocket webSocket, OperationManager manager)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            GetKeepAliveTask(webSocket, result);

            while (!result.CloseStatus.HasValue)
            {
                var text = System.Text.Encoding.UTF8.GetString(buffer);
                var input = JsonConvert.DeserializeObject<OperationMessage>(text);

                var type = MessageTypes.ClientTypes.FirstOrDefault(e => e.Value == input.Type);
                if (type.Value != null)
                {
                    await handlers[type.Key].Handle(webSocket, manager, input);
                }

                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            return result;
        }

        private static void GetKeepAliveTask(WebSocket webSocket, WebSocketReceiveResult result)
        {
            var keepAliveTask = Task.Run(async () =>
            {
                await Task.Yield();

                while (!result.CloseStatus.HasValue)
                {
                    await Task.Delay(1000);
                    await webSocket.SendResponse(MessageType.GQL_CONNECTION_KEEP_ALIVE);
                }
            });
        }
    }
}
