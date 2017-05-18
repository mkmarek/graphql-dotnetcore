namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class FragmentsOnCompositeTypes : IValidationRule
    {
        public IEnumerable<GraphQLException> Validate(GraphQLDocument document, IGraphQLSchema schema)
        {
            var visitor = new FragmentsOnCompositeTypesVisitor(schema);
            visitor.Visit(document);

            return visitor.Errors;
        }
    }
}
