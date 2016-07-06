namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public interface IValidationRule
    {
        IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema);
    }
}