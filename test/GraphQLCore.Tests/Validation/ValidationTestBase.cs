namespace GraphQLCore.Tests.Validation
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Tests.Schemas;
    using GraphQLCore.Validation;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;

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

        protected GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[] 
                {
                    new PossibleFragmentSpreads(),
                    new VariablesAreInputTypes(),
                    new ProvidedNonNullArguments(),
                    new ScalarLeafs(),
                    new ArgumentsOfCorrectType()
                });
        }
    }
}
