namespace GraphQLCore.Tests.Validation.Rules
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

            ErrorAssert.AreEqual("Field \"nonNullIntArgField\" argument \"nonNullIntArg\" of type \"Int!\" is required but not provided.",
                errors.Single(), 4, 21);
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

            ErrorAssert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg1\" of type \"Int!\" is required but not provided.",
                errors.ElementAt(0), 4, 21);
            ErrorAssert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg2\" of type \"Int!\" is required but not provided.",
                errors.ElementAt(1), 4, 21);
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

            ErrorAssert.AreEqual("Field \"nonNullIntMultipleArgsField\" argument \"arg2\" of type \"Int!\" is required but not provided.",
                errors.Single(), 4, 21);
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

            ErrorAssert.AreEqual("Directive \"include\" argument \"if\" of type \"Boolean!\" is required but not provided.",
                errors.ElementAt(0), 3, 33);
            ErrorAssert.AreEqual("Directive \"skip\" argument \"if\" of type \"Boolean!\" is required but not provided.",
                errors.ElementAt(1), 4, 33);
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