namespace GraphQLCore.Execution
{
    internal class FieldContext<T> : IContext<T>
    {
        public T Instance { get; private set; }

        public FieldContext(T instance)
        {
            this.Instance = instance;
        }
    }
}
