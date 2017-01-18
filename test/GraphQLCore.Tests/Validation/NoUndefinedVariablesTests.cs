namespace GraphQLCore.Tests.Validation
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

            Assert.AreEqual("Variable \"$c\" is not defined by operation \"foo\".", errors.Single().Message);
        }

        [Test]
        public void OneVariableUndefinedInAnonymousOperation_ExpectsSingleError()
        {
            var errors = Validate(@"
            query {
                bar(a: $a)
            }
            ");

            Assert.AreEqual("Variable \"$a\" is not defined.", errors.Single().Message);
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
            Assert.AreEqual("Variable \"$arg\" is not defined by operation \"a\".", errors.ElementAt(0).Message);
            Assert.AreEqual("Variable \"$arg1\" is not defined by operation \"a\".", errors.ElementAt(1).Message);
            Assert.AreEqual("Variable \"$arg2\" is not defined by operation \"a\".", errors.ElementAt(2).Message);
            Assert.AreEqual("Variable \"$a\" is not defined by operation \"b\".", errors.ElementAt(3).Message);
            Assert.AreEqual("Variable \"$c\" is not defined by operation \"b\".", errors.ElementAt(4).Message);
        }
    }
}
