namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class FieldsOnCorrectType : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new FieldsOnCorrectTypeVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}
