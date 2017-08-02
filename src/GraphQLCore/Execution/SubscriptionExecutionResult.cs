namespace GraphQLCore.Execution
{
    using Newtonsoft.Json;

    public class SubscriptionExecutionResult : ExecutionResult
    {
        [JsonIgnore]
        public override dynamic Data { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }
    }
}