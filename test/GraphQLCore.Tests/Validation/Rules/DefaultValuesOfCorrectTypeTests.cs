namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class DefaultValuesOfCorrectTypeTests : ValidationTestBase
    {
        [Test]
        public void GoodDefaultValueForVariable_ExpectsNoError()
        {
            var errors = Validate(@"
            query foo($intVar: Int = 1) {
                bar(a: $intVar)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DefaultValueForRequiredVariable_ExpectsSingleError()
        {
            var errors = Validate(@"
            query foo($intVar: Int! = 1) {
                bar(a: $intVar)
            }
            ");

            ErrorAssert.AreEqual("Variable \"$intVar\" of type \"Int!\" is required and will not use the default value. Perhaps you meant to use type \"Int\".",
                errors.Single(), 2, 39);
        }

        [Test]
        public void BadValueForDefaultVariable_ExpectsSingleError() {
            var errors = Validate(@"
            query foo($intVar: Int = ""1"") {
                bar(a: $intVar)
            }
            ");

            ErrorAssert.AreEqual("Variable \"$intVar\" of type \"Int\" has invalid default value \"1\".\nExpected type \"Int\", found \"1\".",
                errors.Single(), 2, 38);
        }

        [Test]
        public void BadValueForDefaultRequiredVariable_ExpectsMultipleErrors()
        {
            var errors = Validate(@"
            query foo($intVar: Int! = ""1"") {
                bar(a: $intVar)
            }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("Variable \"$intVar\" of type \"Int!\" is required and will not use the default value. Perhaps you meant to use type \"Int\".",
                errors.ElementAt(0), 2, 39);
            ErrorAssert.AreEqual("Variable \"$intVar\" of type \"Int!\" has invalid default value \"1\".\nExpected type \"Int\", found \"1\".",
                errors.ElementAt(1), 2, 39);
        }

        [Test]
        public void BadValuesForMultipleDefaultVariables_ExpectsMultipleErrors()
        {
            var errors = Validate(@"
            query foo($intVar: Int, $listVar: [Int] = [1, ""1"", 0.5, [1, 2, 3]]) {
                bar(a: $intVar)
                sum(arg: $listVar)
            }
            ");

            var errorLines = errors.Single().Message.Split('\n');

            Assert.AreEqual("Variable \"$listVar\" of type \"[Int]\" has invalid default value [1, \"1\", 0.5, [1, 2, 3]].", errorLines[0]);
            Assert.AreEqual("In element #1: Expected type \"Int\", found \"1\".", errorLines[1]);
            Assert.AreEqual("In element #2: Expected type \"Int\", found 0.5.", errorLines[2]);
            Assert.AreEqual("In element #3: Expected type \"Int\", found [1, 2, 3].", errorLines[3]);

            ErrorAssert.AreEqual("Variable \"$listVar\" of type \"[Int]\" has invalid default value [1, \"1\", 0.5, [1, 2, 3]].\nIn element #1: Expected type \"Int\", found \"1\".\nIn element #2: Expected type \"Int\", found 0.5.\nIn element #3: Expected type \"Int\", found [1, 2, 3].",
                errors.Single(), 2, 55);
        }
    }
}
