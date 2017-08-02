namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Complex;
    using NUnit.Framework;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQLCore.Events;
    using System.Threading.Tasks;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Execution;

    [TestFixture]
    public class ExecutionContext_Subscription
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_ReturnsCorrectResult()
        {
            var result = this.schema.Execute("subscription { test }", null, null, "1", "0") as SubscriptionExecutionResult;

            Assert.AreEqual("0", result.SubscriptionId);
        }

        [Test]
        public void Execute_ReturnsValueWithCorrectSubIdAfterMutationIsInvoked()
        {
            ExecutionResult result = null;
            string subId = null;
            string cliId = null;

            this.schema.Execute("subscription { test }", null, null, "1", "0");

            this.schema.OnSubscriptionMessageReceived += (sender, e) =>
            {
                result = e.Data as ExecutionResult;
                subId = e.SubscriptionId;
                cliId = e.ClientId;
            };
            this.schema.Execute("mutation { test }");

            Assert.AreEqual(42, result.Data.test);
            Assert.AreEqual("1", cliId);
            Assert.AreEqual("0", subId);
        }

        [Test]
        public void Execute_DoesntReturnDataWhenUserUnsubscribed()
        {
            ExecutionResult result = null;

            this.schema.Execute("subscription { test }", null, null, "1", "0");
            this.schema.Unsubscribe("1", "0");

            this.schema.OnSubscriptionMessageReceived += (sender, e) =>
            {
                if (e.ClientId == "1" && e.SubscriptionId == "0")
                {
                    result = e.Data as ExecutionResult;
                }
            };

            this.schema.Execute("mutation { test }");

            Assert.IsNull(result);
        }

        [Test]
        public void Execute_ThorwsErrorWhenInvokingSubscriptionWithoutSubscriptionId()
        {
            var result = this.schema.Execute("subscription { test }");

            var errors = result.Errors;
            Assert.IsInstanceOf<GraphQLException>(errors.Single());
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var subscriptionRootType = new SubscriptionType();
            var mutationType = new MutationType();

            this.schema.AddKnownType(subscriptionRootType);
            this.schema.AddKnownType(mutationType);

            this.schema.Subscription(subscriptionRootType);
            this.schema.Mutation(mutationType);
        }

        private class SubscriptionType : GraphQLSubscriptionType
        {
            public SubscriptionType() : base("Subscription", "", new InMemoryEventBus())
            {
                this.Field("test", () => 42).OnChannel("test");
            }
        }

        private class MutationType : GraphQLObjectType
        {
            public MutationType() : base("Mutation", "")
            {
                this.Field("test", () => 42).OnChannel("test");
            }
        }
    }
}
