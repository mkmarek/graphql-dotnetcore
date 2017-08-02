namespace GraphQLCore.Execution
{
    using System.Collections;
    using Newtonsoft.Json;

    public class PartialExecutionResult : ExecutionResult
    {
        [JsonProperty(PropertyName = "path", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable Path { get; set; }
    }
}