namespace GraphQLCore.Tests.Validation.Rules
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

            ErrorAssert.StartsWith("Unknown type \"JumbledUpLetters\".",
                jumbledUpLettersError, 2, 31);
            ErrorAssert.StartsWith("Unknown type \"Badger\".",
                badgerError, 5, 54);
            ErrorAssert.StartsWith("Unknown type \"ComplicatedObjectTypeee\". Did you mean \"ComplicatedObjectType\"",
                complicatedObjectArgFieldddddddError, 8, 39);
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

            ErrorAssert.AreEqual("Unknown type \"NotInTheSchema\".", errors.Single(), 12, 33);
        }
    }
}