namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    public class LoneAnonymousOperationTests : ValidationTestBase
    {
        [Test]
        public void AnonymousOperationWithFragment()
        {
            var errors = this.Validate(@"
            {
                ...Foo
            }
            fragment Foo on QueryRoot {
                field { foo }
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleAnonymousOperations()
        {
            var errors = this.Validate(@"
            {
                field { foo }
            }
            {
                field { foo }
            }");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("This anonymous operation must be the only defined operation.", errors.ElementAt(0), 2, 13);
            ErrorAssert.AreEqual("This anonymous operation must be the only defined operation.", errors.ElementAt(1), 5, 13);
        }

        [Test]
        public void AnonymousOperationWithMutation()
        {
            var errors = this.Validate(@"
            {
                field { foo }
            }
            mutation Foo {
                field
            }");

            ErrorAssert.AreEqual("This anonymous operation must be the only defined operation.", errors.Single(), 2, 13);
        }

        [Test]
        public void AnonymousOperationWithSubscription()
        {
            var errors = this.Validate(@"
            {
                field { foo }
            }
            subscription Foo {
                field { foo }
            }");

            ErrorAssert.AreEqual("This anonymous operation must be the only defined operation.", errors.Single(), 2, 13);
        }
    }
}
