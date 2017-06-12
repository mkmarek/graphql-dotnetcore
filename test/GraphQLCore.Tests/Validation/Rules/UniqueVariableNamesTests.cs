namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class UniqueVariableNamesTests : ValidationTestBase
    {
        [Test]
        public void UniqueVariableNames()
        {
            var errors = this.Validate(@"
            query A($x: Int, $y: String) { __typename }
            query B($x: String, $y: Int) { __typename }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DuplicateVariableNames()
        {
            var errors = this.Validate(@"
            query A($x: Int, $x: Int, $x: String) { __typename }
            query B($x: String, $x: Int) { __typename }
            query C($x: Int, $x: Int) { __typename }");

            Assert.AreEqual(4, errors.Count());

            ErrorAssert.AreEqual("There can be only one variable named \"x\".",
                errors.ElementAt(0), new[] { 2, 22 }, new[] { 2, 31 });
            ErrorAssert.AreEqual("There can be only one variable named \"x\".",
                errors.ElementAt(1), new[] { 2, 22 }, new[] { 2, 40 });
            ErrorAssert.AreEqual("There can be only one variable named \"x\".",
                errors.ElementAt(2), new[] { 3, 22 }, new[] { 3, 34 });
            ErrorAssert.AreEqual("There can be only one variable named \"x\".",
                errors.ElementAt(3), new[] { 4, 22 }, new[] { 4, 31 });
        }
    }
}
