namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class KnownTypeNamesTests : ValidationTestBase
    {
        [Test]
        public void UnknownTypeNamesAreInvalid()
        {
            var errors = this.Validate(@"
              query Foo($var: JumbledUpLetters) {
                complicatedArgs {
                  intArgField
                  complicatedObjectArgField { ... on Badger { name }, ...complFields }
                }
              }
              fragment complFields on ComplicatedObjectTypeee {
                booleanField
              }
            ");

            var jumbledUpLettersError = errors.ElementAt(0);
            var badgerError = errors.ElementAt(1);
            var complicatedObjectArgFieldddddddError = errors.ElementAt(2);

            Assert.AreEqual("Unknown type \"JumbledUpLetters\"", jumbledUpLettersError.Message);
            Assert.AreEqual("Unknown type \"Badger\"", badgerError.Message);
            Assert.IsTrue(
                complicatedObjectArgFieldddddddError.Message.StartsWith(
                    "Unknown type \"ComplicatedObjectTypeee\" Did you mean \"ComplicatedObjectType\""));
        }
    }
}