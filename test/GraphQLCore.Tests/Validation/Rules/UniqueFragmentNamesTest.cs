namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class UniqueFragmentNamesTest : ValidationTestBase
    {
        [Test]
        public void UniqueFragmentNames()
        {
            var errors = this.Validate(@"
                fragment Foo on SimpleObjectType {
                    booleanField
                }
                fragment Bar on SimpleObjectType {
                    booleanField
                }
            ");
            Assert.IsEmpty(errors);
        }

        [Test]
        public void DuplicateFragmentNames()
        {
            var errors = this.Validate(@"
                fragment Foo on SimpleObjectType {
                    booleanField
                }
                fragment Foo on SimpleObjectType {
                    booleanField
                }
            ");

            ErrorAssert.AreEqual("There can be only one fragment named \"Foo\".",
                errors.Single(), new[] { 2, 26 }, new[] { 5, 26 });
        }
    }
}
