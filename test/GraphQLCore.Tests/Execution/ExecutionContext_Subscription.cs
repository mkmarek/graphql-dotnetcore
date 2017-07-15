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

    [TestFixture]
    public class ExecutionContext_Subscription
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_ReturnsCorrectResult()
        {
            dynamic result = this.schema.Execute("subscription { test }", null, null, "1", 0);

            Assert.AreEqual(0, result.data.subscriptionId);
            Assert.AreEqual("1", result.data.clientId);
        }

        [Test]
        public void Execute_ReturnsValueWithCorrectSubIdAfterMutationIsInvoked()
        {
            dynamic result = null;
            int? subId = null;
            string cliId = null;

            this.schema.Execute("subscription { test }", null, null, "1", 0);

            this.schema.OnSubscriptionMessageReceived += async (string clientId, int subscriptionId, dynamic data) => {
                await Task.Yield();

                result = data;
                subId = subscriptionId;
                cliId = clientId;
            };

            this.schema.Execute("mutation { test }");

            Assert.AreEqual(42, result.data.test);
            Assert.AreEqual("1", cliId);
            Assert.AreEqual(0, subId);
        }

        [Test]
        public void Execute_DoesntReturnDataWhenUserUnsubscribed()
        {
            dynamic result = null;

            this.schema.Execute("subscription { test }", null, null, "1", 0);
            this.schema.Unsubscribe("1", 0);

            this.schema.OnSubscriptionMessageReceived += async (string clientId, int subscriptionId, dynamic data) => {
                await Task.Yield();

                if (clientId == "1" && subscriptionId == 0)
                {
                    result = data;
                }
            };

            this.schema.Execute("mutation { test }");

            Assert.IsNull(result);
        }

        [Test]
        public void Execute_ThorwsErrorWhenInvokingSubscriptionWithoutSubscriptionId()
        {
            var result = this.schema.Execute("subscription { test }");

            var errors = result.errors as IList<GraphQLException>;
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
