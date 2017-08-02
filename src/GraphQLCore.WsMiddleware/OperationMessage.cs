namespace GraphQLCore.WsMiddleware
{
    using Newtonsoft.Json;

    public class OperationMessage
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public object Payload { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
