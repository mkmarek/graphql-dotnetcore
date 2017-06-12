namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;

    public class SingleFieldSubscriptionsTests : ValidationTestBase
    {
        [Test]
        public void ValidSubscription()
        {
            var errors = this.Validate(@"
            subscription ImportantEmails {
                importantEmails
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SubscriptionFailsWithMoreThanOneRootField()
        {
            var errors = this.Validate(@"
subscription ImportantEmails {
    importantEmails
    notImportantEmails
}
            ");

            ErrorAssert.AreEqual("Subscription \"ImportantEmails\" must select only one top level field.",
                errors.Single(), 4, 5);
        }

        [Test]
        public void SubscriptionFailsWithMoreThanOneRootFieldIncludingIntrospection()
        {
            var errors = this.Validate(@"
subscription ImportantEmails {
    importantEmails
    __typename
}
            ");

            ErrorAssert.AreEqual("Subscription \"ImportantEmails\" must select only one top level field.", 
                errors.Single(), 4, 5);
        }

        [Test]
        public void SubscriptionFailsWithManyMoreThanOneRootField()
        {
            var errors = this.Validate(@"
subscription ImportantEmails {
    importantEmails
    notImportantEmails
    spamEmails
}
            ");

            ErrorAssert.AreEqual("Subscription \"ImportantEmails\" must select only one top level field.",
                errors.Single(), new int[] { 4, 5 }, new int[] { 5, 5 });
        }

        [Test]
        public void AnonymousSubscriptionFailsWithMoreThanOneRootField()
        {
            var errors = this.Validate(@"
subscription {
    importantEmails
    notImportantEmails
}
            ");

            ErrorAssert.AreEqual("Anonymous Subscription must select only one top level field.",
                errors.Single(), 4, 5);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new SingleFieldSubscriptions()
                });
        }
    }
}
