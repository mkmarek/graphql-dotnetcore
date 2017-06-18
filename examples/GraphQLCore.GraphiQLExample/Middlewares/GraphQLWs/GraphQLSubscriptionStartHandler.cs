using GraphQLCore.Exceptions;
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
    public class GraphQLSubscriptionStartHandler : IGraphQLWsHandler
    {
        public async Task Handle(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            try
            {
                await Subscribe(socket, clientId, schema, input);
            }
            catch (GraphQLValidationException ex)
            {
                await SendResponseToGraphQLValidationException(socket, input.Id.Value, ex);
            }
            catch (GraphQLException ex)
            {
                await SendResponseToGraphQLException(socket, input.Id.Value, ex);
            }
            catch (Exception ex)
            {
                await SendReponseToException(socket, input.Id.Value, ex);
            }
        }

        private static async Task Subscribe(WebSocket socket, string clientId, IGraphQLSchema schema, WsInputObject input)
        {
            var data = schema.Execute(input.Query, null, null, clientId, input.Id.Value);

            var dataString = JsonConvert.SerializeObject(new { id = input.Id, type = "subscription_success" });
            var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

            await socket.SendAsync(
                new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task SendResponseToGraphQLValidationException(WebSocket socket, int id, GraphQLValidationException ex)
        {
            var dataString = JsonConvert.SerializeObject(new
            {
                id,
                type = "subscription_fail",
                payload = new
                {
                    errors = ex.Errors
                }
            });

            await SendResponse(socket, dataString);
        }

        private static async Task SendResponseToGraphQLException(WebSocket socket, int id, GraphQLException ex)
        {
            var dataString = JsonConvert.SerializeObject(new
            {
                id,
                type = "subscription_fail",
                payload = new
                {
                    errors = new dynamic[] { new { message = ex.Message + "\n" + ex.StackTrace } }
                }
            });

            await SendResponse(socket, dataString);
        }

        private static async Task SendReponseToException(WebSocket socket, int id, Exception ex)
        {
            var dataString = JsonConvert.SerializeObject( new
            {
                id,
                type = "subscription_fail",
                payload = new
                {
                    errors = new dynamic[] { new { message = ex.Message + "\n" + ex.StackTrace } }
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
