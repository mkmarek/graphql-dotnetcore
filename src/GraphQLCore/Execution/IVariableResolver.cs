namespace GraphQLCore.Execution
{
    using Language.AST;
    using Type;

    public interface IVariableResolver
    {
        Result GetValue(string variableName);

        Result GetValue(GraphQLVariable value);
    }
}