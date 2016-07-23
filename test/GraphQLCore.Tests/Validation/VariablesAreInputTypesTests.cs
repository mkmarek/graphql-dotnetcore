namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Validation;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using Schemas;
    using System.Linq;

    [TestFixture]
    public class VariablesAreInputTypesTests
    {
        private VariablesAreInputTypes variablesAreInputTypes;
        private ValidationContext validationContext;
        private TestSchema validationTestSchema;

        [SetUp]
        public void SetUp()
        {
            this.variablesAreInputTypes = new VariablesAreInputTypes();
            this.validationTestSchema = new TestSchema();

            this.validationContext = new ValidationContext();
        }

        [Test]
        public void InputTypesAreValid()
        {
            var errors = Validate(@"
            query Foo($a: String, $b: [Boolean!]!, $c: ComplicatedInputObjectType) {
                field(a: $a, b: $b, c: $c)
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void OutputTypesAreInvalid()
        {
            var errors = Validate(@"
            query Foo($a: ComplicatedObjectType, $b: [[ComplicatedObjectType!]]!, $c: ComplicatedInterfaceType) {
                field(a: $a, b: $b, c: $c)
            }
            ");

            Assert.AreEqual(3, errors.Count());
            Assert.AreEqual("Variable \"$a\" cannot be non-input type \"ComplicatedObjectType\".", errors.ElementAt(0).Message);
            Assert.AreEqual("Variable \"$b\" cannot be non-input type \"[[ComplicatedObjectType!]]!\".", errors.ElementAt(1).Message);
            Assert.AreEqual("Variable \"$c\" cannot be non-input type \"ComplicatedInterfaceType\".", errors.ElementAt(2).Message);
        }

        private static GraphQLDocument GetAst(string body)
        {
            return new Parser(new Lexer()).Parse(new Source(body));
        }

        private GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[] { this.variablesAreInputTypes });
        }
    }
}