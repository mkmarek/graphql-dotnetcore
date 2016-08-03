namespace GraphQLCore.Validation
{
    using Language.AST;
    using Type;

    public class VariableUsage
    {
        public GraphQLBaseType ArgumentType { get; set; }
        public GraphQLVariable Variable { get; set; }
    }
}
