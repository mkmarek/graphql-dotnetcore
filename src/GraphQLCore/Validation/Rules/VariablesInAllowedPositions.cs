namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class VariablesInAllowedPositions : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new VariablesInAllowedPositionsVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}