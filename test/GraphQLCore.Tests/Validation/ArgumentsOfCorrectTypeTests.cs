namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Validation;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using Schemas;
    using System.Linq;

    [TestFixture]
    public class ArgumentsOfCorrectTypeTests
    {
        private ArgumentsOfCorrectType argumentsOfCorrectType;
        private ValidationContext validationContext;
        private TestSchema validationTestSchema;

        [SetUp]
        public void SetUp()
        {
            this.argumentsOfCorrectType = new ArgumentsOfCorrectType();
            this.validationTestSchema = new TestSchema();

            this.validationContext = new ValidationContext();
        }

        [Test]
        public void Validate_GoodBooleanValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: true)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodEnumValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: TAN)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodFloatValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: 1.2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodIntValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodStringValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: " + "\"foo\"" + @")
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_IntIntoFloat_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: 2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_IntIntoString_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: 1)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_NoArgOnNonOpitonalArg_ExpectsError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    nonNullIntArgField
                }
            }
            ");

            Assert.AreEqual(1, errors.Count());
        }

        private static GraphQLDocument GetAst(string body)
        {
            return new Parser(new Lexer()).Parse(new Source(body));
        }

        private GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(GetAst(body), this.validationTestSchema, new IValidationRule[] { this.argumentsOfCorrectType });
        }
    }
}