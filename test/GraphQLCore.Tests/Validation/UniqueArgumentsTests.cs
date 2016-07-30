namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class UniqueArgumentsTests : ValidationTestBase
    {
        [Test]
        public void NoArgumentsOnField()
        {
            var errors = this.Validate(@"
            {
                foo
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NoArgumentsOnDirective()
        {
            var errors = this.Validate(@"
            {
                foo @directive
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ArgumentOnField()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ArgumentOnDirective()
        {
            var errors = this.Validate(@"
            {
                foo @directive(a: 1)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameArgumentOnTwoFields()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1)
                bar(a: 1)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameArgumentOnFieldAndDirective()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1) @directive(a: 1)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameArgumentOnTwoDirectives()
        {
            var errors = this.Validate(@"
            {
                foo @directive1(a: 1) @directive2(a: 1)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleArgumentsOnField()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1, b: 2, c: 3)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleDirectiveArguments()
        {
            var errors = this.Validate(@"
            {
                foo @directive(a: 1, b: 2, c: 3)
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DuplicateFieldArguments()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1, a: 2)
            }");

            Assert.AreEqual("There can be only one argument named \"a\".", errors.Single().Message);
        }

        [Test]
        public void ManyDuplicateFieldArguments()
        {
            var errors = this.Validate(@"
            {
                foo(a: 1, a: 2, a : 3)
            }");

            Assert.AreEqual("There can be only one argument named \"a\".", errors.ElementAt(0).Message);
            Assert.AreEqual("There can be only one argument named \"a\".", errors.ElementAt(1).Message);
        }

        [Test]
        public void DuplicateDirectiveArguments()
        {
            var errors = this.Validate(@"
            {
                foo @directive(a: 1, a: 2)
            }");

            Assert.AreEqual("There can be only one argument named \"a\".", errors.Single().Message);
        }

        [Test]
        public void ManyDuplicateDirectiveArguments()
        {
            var errors = this.Validate(@"
            {
                foo @directive(a: 1, a: 2, a: 3)
            }");

            Assert.AreEqual("There can be only one argument named \"a\".", errors.ElementAt(0).Message);
            Assert.AreEqual("There can be only one argument named \"a\".", errors.ElementAt(1).Message);
        }
    }
}
