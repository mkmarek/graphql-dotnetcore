namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class UniqueOperationNamesTests : ValidationTestBase
    {
        [Test]
        public void NoOperations()
        {
            var errors = this.Validate(@"
            fragment fragA on ComplicatedObjectType {
                intField
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void OneAnonymousOperation()
        {
            var errors = this.Validate(@"
            {
                foo
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void OneNamedOperation()
        {
            var errors = this.Validate(@"
            query FooOp {
                foo
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleOperations()
        {
            var errors = this.Validate(@"
            query FooOp {
                foo
            }
            query BarOp {
                foo
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleOperationsOfDifferentTypes()
        {
            var errors = this.Validate(@"
            query Foo {
                foo
            }
            mutation Bar {
                foo
            }
            subscription Baz {
                foo
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FragmentAndOperationSameName()
        {
            var errors = this.Validate(@"
            query Foo {
                foo
            }
            fragment Foo on ComplicatedObjectType {
                intField
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleOperationsOfTheSameName()
        {
            var errors = this.Validate(@"
            query Foo {
                foo
            }
            query Foo {
                foo
            }");

            ErrorAssert.AreEqual("There can only be one operation named \"Foo\".",
                errors.Single(), new[] { 2, 19 }, new[] { 5, 19 });
        }

        [Test]
        public void MultipleOperationsOfTheSameNameDifferentTypesWithMutation()
        {
            var errors = this.Validate(@"
            query Foo {
                foo
            }
            mutation Foo {
                foo
            }");

            ErrorAssert.AreEqual("There can only be one operation named \"Foo\".",
                errors.Single(), new[] { 2, 19 }, new[] { 5, 22 });
        }

        [Test]
        public void MultipleOperationsOfTheSameNameDifferentTypesWithSubscription()
        {
            var errors = this.Validate(@"
            query Foo {
                foo
            }
            subscription Foo {
                foo
            }");

            ErrorAssert.AreEqual("There can only be one operation named \"Foo\".",
                errors.Single(), new[] { 2, 19 }, new[] { 5, 26 });
        }
    }
}
