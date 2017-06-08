namespace GraphQLCore.Tests.Validation
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class ProvidedNonNullArgumentsTests : ValidationTestBase
    {
        [Test]
        public void MissingOneNonNullableArgument()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    nonNullIntArgField
                }
            }
            ");

            var error = errors.Single();
            Assert.AreEqual("Field \"nonNullIntArgField\" argument \"nonNullIntArg\" of type \"Int!\" is required but not provided.",
                error.Message);
        }

        [Test]
        public void MissingMultipleNonNullableArguments()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    nonNullIntMultipleArgsField
                }
            }
            ");

            Assert.AreEqual(2, errors.Count());

            var error1 = errors.ElementAt(0);
            var error2 = errors.ElementAt(1);

            Assert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg1\" of type \"Int!\" is required but not provided.",
                error1.Message);
            Assert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg2\" of type \"Int!\" is required but not provided.",
                error2.Message);
        }

        [Test]
        public void IncorrectValueAndMissingArgument()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    nonNullIntMultipleArgsField(arg1: "+ "\"one\""+@")
                }
            }
            ");

            var error1 = errors.ElementAt(0);

            Assert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg2\" of type \"Int!\" is required but not provided.",
                error1.Message);
        }

        [Test]
        public void UnknownArguments_ReportsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(unknownArgument: true)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void UnknownDirectives_ReportsNoError()
        {
            var errors = Validate(@"
            {
                foo @unknown
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DirectivesOfValidTypes_ReportsNoError()
        {
            var errors = Validate(@"
            {
                foo @include(if: true)
                bar @skip(if: false)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DirectivesWithMissingTypes_ReportsErrors()
        {
            var errors = Validate(@"
            {
                complicatedArgs @include {
                    intArgField @skip
                }
            }
            ");

            Assert.AreEqual(2, errors.Count());
            Assert.AreEqual("Directive \"include\" argument \"if\" of type \"Boolean!\" is required but not provided.", errors.ElementAt(0).Message);
            Assert.AreEqual("Directive \"skip\" argument \"if\" of type \"Boolean!\" is required but not provided.", errors.ElementAt(1).Message);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new ProvidedNonNullArguments()
                });
        }
    }
}