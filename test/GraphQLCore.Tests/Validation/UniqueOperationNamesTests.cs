namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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

            Assert.AreEqual("There can only be one operation named \"Foo\".", errors.Single().Message);
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

            Assert.AreEqual("There can only be one operation named \"Foo\".", errors.Single().Message);
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

            Assert.AreEqual("There can only be one operation named \"Foo\".", errors.Single().Message);
        }
    }
}
