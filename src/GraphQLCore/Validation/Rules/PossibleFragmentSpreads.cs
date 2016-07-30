namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class PossibleFragmentSpreads : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new PossibleFragmentSpreadsVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}