namespace GraphQLCore.WsMiddleware.Payloads
{
    using System;
    using Newtonsoft.Json;

    public class ErrorPayload : IPayload
    {
        [JsonProperty(PropertyName = "error")]
        public Exception Error { get; set; }
    }
}
