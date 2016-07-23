namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class VariablesAreInputTypes : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new VariablesAreInputTypesVisitor(schema.SchemaRepository);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}