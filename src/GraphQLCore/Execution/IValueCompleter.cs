namespace GraphQLCore.Execution
{
    using System.Threading.Tasks;

    public interface IValueCompleter
    {
        Task<object> CompleteValue(ExecutedField field, object result);
    }
}