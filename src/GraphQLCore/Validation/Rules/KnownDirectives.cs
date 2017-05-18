namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class KnownDirectives : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new KnownDirectivesVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}