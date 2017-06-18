namespace GraphQLCore.Tests.Validation
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Validation;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using Schemas;

    public class ValidationTestBase
    {
        protected ValidationContext validationContext;
        protected TestSchema validationTestSchema;

        [SetUp]
        public void SetUp()
        {
            this.validationTestSchema = new TestSchema();
            this.validationContext = new ValidationContext();
        }

        protected static GraphQLDocument GetAst(string body)
        {
            return new Parser(new Lexer()).Parse(new Source(body));
        }

        protected virtual GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new NoUndefinedVariables(),
                    new DefaultValuesOfCorrectType(),
                    new VariablesInAllowedPositions(),
                    new LoneAnonymousOperation(),
                    new UniqueInputFieldNames(),
                    new UniqueArguments(),
                    new UniqueVariableNames(),
                    new UniqueOperationNames(),
                    new UniqueFragmentNames(),
                    new KnownTypeNames(),
                    new PossibleFragmentSpreads(),
                    new VariablesAreInputTypes(),
                    new ProvidedNonNullArguments(),
                    new ScalarLeafs(),
                    new ArgumentsOfCorrectType(),
                    new KnownDirectives(),
                    new FragmentsOnCompositeTypes(),
                    new UniqueDirectivesPerLocation(),
                    new OverlappingFieldsCanBeMerged()
                });
        }
    }
}
