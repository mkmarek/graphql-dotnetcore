namespace GraphQLCore.Execution
{
    using GraphQLCore.Language.AST;

    public interface IVariableResolver
    {
        object GetValue(string variableName);

        object GetValue(GraphQLVariable value);
    }
}