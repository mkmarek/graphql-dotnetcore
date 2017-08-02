namespace GraphQLCore.Execution
{
    public interface IContext<T>
    {
        T Instance { get; }
    }
}
