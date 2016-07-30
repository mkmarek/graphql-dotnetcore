namespace GraphQLCore.Tests.Validation
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

            Assert.AreEqual("There can be only one variable named \"x\".", errors.ElementAt(0).Message);
            Assert.AreEqual("There can be only one variable named \"x\".", errors.ElementAt(1).Message);
            Assert.AreEqual("There can be only one variable named \"x\".", errors.ElementAt(2).Message);
            Assert.AreEqual("There can be only one variable named \"x\".", errors.ElementAt(3).Message);
        }
    }
}
