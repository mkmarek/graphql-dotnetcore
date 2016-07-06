namespace GraphQLCore.Validation
{
    using Exceptions;
    using Language.AST;
    using Rules;
    using System.Collections.Generic;
    using Type;

    public class ValidationContext
    {
        public GraphQLException[] Validate(
            GraphQLDocument document,
            IGraphQLSchema schema,
            IValidationRule[] validationRules)
        {
            var errors = new List<GraphQLException>();

            foreach (var rule in validationRules)
                errors.AddRange(rule.Validate(document, schema));

            return errors.ToArray();
        }
    }
}