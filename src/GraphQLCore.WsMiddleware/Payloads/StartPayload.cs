namespace GraphQLCore.WsMiddleware.Payloads
{
    using System.Dynamic;
    using Newtonsoft.Json;

    public class StartPayload : IPayload
    {
        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }

        [JsonProperty(PropertyName = "variables")]
        public ExpandoObject Variables { get; set; }

        [JsonProperty(PropertyName = "operationName")]
        public string OperationName { get; set; }
    }
}
