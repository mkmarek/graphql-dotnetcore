namespace GraphQLCore.Execution
{
    using GraphQLCore.Type;

    public class ValueContext<TType, TValue> : IValueContext
        where TValue : class
        where TType : GraphQLObjectTypeBase<TValue>
    {
        public ValueContext(TType type, TValue value)
        {
            this.Type = type;
            this.Value = value;
        }

        public TType Type { get; private set; }
        public TValue Value { get; private set; }

        public GraphQLObjectType GetObjectType()
        {
            return this.Type;
        }

        public object GetValue()
        {
            return this.Value;
        }
    }
}