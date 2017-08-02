namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IArgumentFetcher
    {
        object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments, object parent);
    }
}