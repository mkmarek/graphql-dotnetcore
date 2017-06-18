using GraphQLCore.Exceptions;
using GraphQLCore.GraphiQLExample.Middlewares.GraphQLWs;
using GraphQLCore.Type;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQLCore.GraphiQLExample.Middlewares
{
    public static class GraphQLWsMiddleware
    {
        private static Dictionary<string, IGraphQLWsHandler> handlers = new Dictionary<string, IGraphQLWsHandler>()
        {
            { "init", new GraphQLInitHandler() },
            { "subscription_start", new GraphQLSubscriptionStartHandler() },
            { "subscription_end", new GraphQLSubscriptionEndHandler() }
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
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

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
            var clientId = GenerateClientId();

            var onDataReceived = GetCallback(webSocket, clientId);

            var schema = GetSchema(context, onDataReceived);

            var result = await MainLoop(webSocket, clientId, schema);

            schema.Unsubscribe(clientId);
            schema.OnSubscriptionMessageReceived -= onDataReceived;

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private static string GenerateClientId()
        {
            return Guid.NewGuid().ToString();
        }

        private static IGraphQLSchema GetSchema(HttpContext context, SubscriptionMessageReceived received)
        {
            var schema = context.RequestServices.GetService(typeof(IGraphQLSchema)) as IGraphQLSchema;
            schema.OnSubscriptionMessageReceived += received;
            return schema;
        }

        private static async Task<WebSocketReceiveResult> MainLoop(WebSocket webSocket, string clientId, IGraphQLSchema schema)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            GetKeepAliveTask(webSocket, result);

            while (!result.CloseStatus.HasValue)
            {
                var text = System.Text.Encoding.UTF8.GetString(buffer);
                var input = JsonConvert.DeserializeObject<WsInputObject>(text);

                if (handlers.ContainsKey(input.Type))
                {
                    await handlers[input.Type].Handle(webSocket, clientId, schema, input);
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

                    var dataString = JsonConvert.SerializeObject(new
                    {
                        type = "keepalive"
                    });

                    var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
        }

        private static SubscriptionMessageReceived GetCallback(WebSocket webSocket, string clientId)
        {
            return async (string msgClientId, int subscriptionId, dynamic subscriptionData) =>
            {
                try
                {
                    if (clientId == msgClientId)
                    {
                        var ds = JsonConvert.SerializeObject(new
                        {
                            id = subscriptionId,
                            type = "subscription_data",
                            payload = new
                            {
                                data = subscriptionData
                            }
                        });
                        var db = System.Text.Encoding.UTF8.GetBytes(ds);

                        await webSocket.SendAsync(
                            new ArraySegment<byte>(db), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {

                }
            };
        }
    }
}