namespace GraphQLCore.Tests.Validation
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
                field
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleAnonymousOperations()
        {
            var errors = this.Validate(@"
            {
                field
            }
            {
                field
            }");

            Assert.AreEqual("This anonymous operation must be the only defined operation.", errors.ElementAt(0).Message);
            Assert.AreEqual("This anonymous operation must be the only defined operation.", errors.ElementAt(1).Message);
        }

        [Test]
        public void AnonymousOperationWithMutation()
        {
            var errors = this.Validate(@"
            {
                field
            }
            mutation Foo {
                field
            }");

            Assert.AreEqual("This anonymous operation must be the only defined operation.", errors.Single().Message);
        }

        [Test]
        public void AnonymousOperationWithSubscription()
        {
            var errors = this.Validate(@"
            {
                field
            }
            subscription Foo {
                field
            }");

            Assert.AreEqual("This anonymous operation must be the only defined operation.", errors.Single().Message);
        }
    }
}
