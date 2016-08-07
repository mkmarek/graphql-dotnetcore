using System.Linq;

namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using Exceptions;
    using System.Collections.Generic;
    using GraphQLCore.Validation.Rules;

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
            Assert.AreEqual("There can be only one fragment named \"Foo\".", errors.ElementAt(0).Message);
            Assert.AreEqual(1, errors.Count());
        }
    }
}
