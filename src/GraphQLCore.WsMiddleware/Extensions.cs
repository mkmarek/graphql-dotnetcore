namespace GraphQLCore.WsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Payloads;

    public static class Extensions
    {
        public static async Task SendResponse(this WebSocket socket, string dataString)
        {
            var resultBuffer = System.Text.Encoding.UTF8.GetBytes(dataString);

            await socket.SendAsync(
                new ArraySegment<byte>(resultBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task SendResponse(this WebSocket socket, MessageType type, string id = null, IPayload payload = null)
        {
            var typeValue = MessageTypes.ServerTypes[type];

            IDictionary<string, object> data = new ExpandoObject();
            if (id != null)
                data.Add("id", id);
            if (payload != null)
                data.Add("payload", payload);
            data.Add("type", typeValue);

            await SendResponse(socket, JsonConvert.SerializeObject(data));
        }
    }
}
