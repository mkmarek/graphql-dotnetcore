namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class VariablesAreInputTypesTests : ValidationTestBase
    {
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
    }
}