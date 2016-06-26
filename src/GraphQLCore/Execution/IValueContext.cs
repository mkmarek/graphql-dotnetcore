namespace GraphQLCore.Execution
{
    using GraphQLCore.Type;

    public interface IValueContext
    {
        GraphQLObjectType GetObjectType();

        object GetValue();
    }
}