namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueOperationNames : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new UniqueOperationNamesVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}