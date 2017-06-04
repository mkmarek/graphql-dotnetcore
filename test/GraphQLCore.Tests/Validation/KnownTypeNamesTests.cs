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
            Assert.IsTrue(badgerError.Message.StartsWith("Unknown type \"Badger\""));
            Assert.IsTrue(
                complicatedObjectArgFieldddddddError.Message.StartsWith(
                    "Unknown type \"ComplicatedObjectTypeee\" Did you mean \"ComplicatedObjectType\""));
        }

        [Test]
        public void IgnoresTypeDefinitions()
        {
            var errors = this.Validate(@"
                type NotInTheSchema {
                    field: FooBar
                }
                interface FooBar {
                    field: NotInTheSchema
                }
                union U = A | B
                input Blob {
                    field: UnknownType
                }
                query Foo($var: NotInTheSchema) {
                    user(id: $var) {
                        id
                    }
                }");

            Assert.AreEqual("Unknown type \"NotInTheSchema\"", errors.Single().Message);
        }
    }
}