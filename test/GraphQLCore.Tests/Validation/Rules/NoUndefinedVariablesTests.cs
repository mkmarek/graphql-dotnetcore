namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class NoUndefinedVariablesTests : ValidationTestBase
    {
        [Test]
        public void AllVariablesDefined_ExpectsNoError()
        {
            var errors = Validate(@"
            query foo($a: String, $b: String, $c: String) {
                field(a: $a, b: $b, c: $c) { foo }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void OneVariableUndefined_ExpectsSingleError()
        {
            var errors = Validate(@"
            query foo($a: Int, $b: Int) {
                foo(a: $a, b: $b, c: $c) 
            }
            ");

            ErrorAssert.AreEqual("Variable \"$c\" is not defined by operation \"foo\".",
                errors.Single(), new[] { 3, 38 }, new[] { 2, 13 });
        }

        [Test]
        public void OneVariableUndefinedInAnonymousOperation_ExpectsSingleError()
        {
            var errors = Validate(@"
            query {
                bar(a: $a)
            }
            ");

            ErrorAssert.AreEqual("Variable \"$a\" is not defined.",
                errors.Single(), new[] { 3, 24 }, new[] { 2, 13 });
        }

        [Test]
        public void MultipleVariablesUndefined_ExpectsMultipleErrors()
        {
            var errors = Validate(@"
            query a {
                field(a: $arg, b: $arg1, c: $arg2) { foo }
            }
            query b ($b: Int) {
                foo(a: $a, b: $b, c: $c)
            }
            ");

            Assert.AreEqual(5, errors.Count());

            ErrorAssert.AreEqual("Variable \"$arg\" is not defined by operation \"a\".",
                errors.ElementAt(0), new[] { 3, 26 }, new[] { 2, 13 });
            ErrorAssert.AreEqual("Variable \"$arg1\" is not defined by operation \"a\".",
                errors.ElementAt(1), new[] { 3, 35 }, new[] { 2, 13 });
            ErrorAssert.AreEqual("Variable \"$arg2\" is not defined by operation \"a\".",
                errors.ElementAt(2), new[] { 3, 45 }, new[] { 2, 13 });
            ErrorAssert.AreEqual("Variable \"$a\" is not defined by operation \"b\".",
                errors.ElementAt(3), new[] { 6, 24 }, new[] { 5, 13 });
            ErrorAssert.AreEqual("Variable \"$c\" is not defined by operation \"b\".",
                errors.ElementAt(4), new[] { 6, 38 }, new[] { 5, 13 });
        }
    }
}
