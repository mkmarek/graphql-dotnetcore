namespace GraphQLCore.Execution
{
    using Exceptions;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class ExecutionResult
    {
        [JsonProperty(PropertyName = "data")]
        public virtual dynamic Data { get; set; }

        [JsonProperty(PropertyName = "errors", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<GraphQLException> Errors { get; set; }

        [JsonIgnore]
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Backwards compatibility")]
        public dynamic data => this.Data;

        [JsonIgnore]
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Backwards compatibility")]
        public dynamic errors => this.Errors;
    }
}