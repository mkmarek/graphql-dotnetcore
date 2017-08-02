namespace GraphQLCore.Execution
{
    using System.Threading.Tasks;

    public interface IFieldExpression
    {
        Task<object> GetResult();
        object GetResultSync();
    }
}