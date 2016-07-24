namespace GraphQLCore.GraphiQLExample.Models
{
    public class GraphiQLInput
    {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public string Variables { get; set; }
    }
}