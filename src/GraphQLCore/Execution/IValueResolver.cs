namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IValueResolver
    {
        object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName);
        object GetValue(GraphQLValue value);
        object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments);
    }
}
