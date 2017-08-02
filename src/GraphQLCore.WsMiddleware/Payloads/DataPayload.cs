namespace GraphQLCore.WsMiddleware.Payloads
{
    using System.Collections.Generic;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Execution;
    using Newtonsoft.Json;

    public class DataPayload : IPayload
    {
        [JsonProperty(PropertyName = "data")]
        public ExecutionResult Data { get; set; }

        [JsonProperty(PropertyName = "errors", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<GraphQLException> Errors { get; set; }
    }
}
