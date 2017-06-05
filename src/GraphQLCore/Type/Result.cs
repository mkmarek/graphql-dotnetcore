namespace GraphQLCore.Type
{
    public class Result
    {
        public bool IsValid { get; private set; }
        public object Value { get; }

        public Result(object value)
        {
            this.IsValid = true;
            this.Value = value;
        }

        public static readonly Result Invalid = new Result(null)
        {
            IsValid = false
        };
    }
}
