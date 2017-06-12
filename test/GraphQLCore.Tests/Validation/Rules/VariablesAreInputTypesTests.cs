namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
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
                field(a: $a, b: $b, c: $c) { foo }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void OutputTypesAreInvalid()
        {
            var errors = Validate(@"
            query Foo($a: ComplicatedObjectType, $b: [[ComplicatedObjectType!]]!, $c: ComplicatedInterfaceType) {
                field(a: $a, b: $b, c: $c) { foo }
            }
            ");

            Assert.AreEqual(3, errors.Count());

            ErrorAssert.AreEqual("Variable \"$a\" cannot be non-input type \"ComplicatedObjectType\".",
                errors.ElementAt(0), 2, 27);
            ErrorAssert.AreEqual("Variable \"$b\" cannot be non-input type \"[[ComplicatedObjectType!]]!\".",
                errors.ElementAt(1), 2, 54);
            ErrorAssert.AreEqual("Variable \"$c\" cannot be non-input type \"ComplicatedInterfaceType\".",
                errors.ElementAt(2), 2, 87);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new VariablesAreInputTypes(),
                });
        }
    }
}