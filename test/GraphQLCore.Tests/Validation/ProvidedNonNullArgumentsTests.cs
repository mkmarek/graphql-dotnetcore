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
    }
}