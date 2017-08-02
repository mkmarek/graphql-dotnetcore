namespace GraphQLCore.Execution
{
    using System.Collections;
    using System.Threading.Tasks;

    public class FieldExecution
    {
        public IEnumerable Path { private get; set; }
        public Task<dynamic> Result { private get; set; }

        public async Task<ExecutionResult> GetResult()
        {
            return new PartialExecutionResult()
            {
                Path = this.Path,
                Data = await this.Result
            };
        }
    }
}
