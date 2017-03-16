namespace GraphQLCore.GraphiQLExample.Models
{
    public class GraphiQLInput
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public dynamic Variables { get; set; }
    }
}
