using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using GraphQLCore.Type.Complex;
using GraphQLCore.Events;
using GraphQLCore.Execution;
using GraphQLCore.Type;
using System.Threading.Tasks;

namespace GraphQLCore.Tests.Type
{
    [TestFixture]
    public class GraphQLSubscriptionTypeTests
    {
        private InMemoryEventBus eventBus;
        private TestSchema schema;

        public class Message
        {
            public string Author { get; set; }
            public string Content { get; set; }
        }

        public class MessageSchemaType : GraphQLObjectType<Message>
        {
            public MessageSchemaType() : base("Message", string.Empty)
            {
                this.Field("author", e => e.Author);
                this.Field("content", e => e.Content);
            }
        }

        public class MutationRootType : GraphQLObjectType
        {
            public MutationRootType() : base("Mutation", string.Empty)
            {
                this.Field("testMutation", () => new Message() { Author = "other", Content = "blabla" })
                    .OnChannel("testingSsubscriptionChannel");
                this.Field("testMutation1", () => new Message() { Author = "other", Content = "blabla" })
                    .OnChannel("testingSsubscriptionChannel1");
            }
        }

        public class SubscriptionRootType : GraphQLSubscriptionType
        {
            public SubscriptionRootType(IEventBus eventBus) : base("Subscription", string.Empty, eventBus)
            {
                this.Field("testSub", (string author) => GetMessage(author))
                    .OnChannel("testingSsubscriptionChannel")
                    .WithSubscriptionFilter((IContext<Message> ctx, string author) => SubscriptionFilter(ctx, author));
            }

            private static bool SubscriptionFilter(IContext<Message> ctx, string author)
            {
                return ctx.Instance.Author == author;
            }

            private static Message GetMessage(string author)
            {
                return new Message() { Author = author, Content = "test" };
            }
        }

        public class TestSchema : GraphQLSchema
        {
            public TestSchema(IEventBus eventBus)
            {
                var subscriptionRootType = new SubscriptionRootType(eventBus);
                var mutationRootType = new MutationRootType();

                this.AddKnownType(mutationRootType);
                this.AddKnownType(subscriptionRootType);
                this.AddKnownType(new MessageSchemaType());

                this.Mutation(mutationRootType);
                this.Subscription(subscriptionRootType);
            }
        }

        [SetUp]
        public void Setup()
        {
            this.eventBus = new InMemoryEventBus();
            this.schema = new TestSchema(this.eventBus);
        }

        [Test]
        public void SubscriptionReturnsSubscriptionIdentifierAsLongInt()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub {
                    content
                }
            }");

            Assert.IsInstanceOf<long>(result.testSub);
        }

        [Test]
        public async Task CallingSubscriptionFromGraphQLPropagatesSubscriptionToEventBus()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub(author : ""test"") {
                    content
                }
            }");

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();

                eventArgs = args;
            };

            await this.eventBus.Publish(new Message() { Author = "test", Content = "blabla" },
                "testingSsubscriptionChannel");

            Assert.IsNotNull(eventArgs);
        }

        [Test]
        public async Task DoesNotFireWhenFilterIsNotEvaluatedToTrue()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub(author : ""other"") {
                    content
                }
            }");

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();
                eventArgs = args;
            };

            await this.eventBus.Publish(new Message() { Author = "test", Content = "blabla" },
                "testingSsubscriptionChannel");

            Assert.IsNull(eventArgs);
        }

        [Test]
        public void InvokesResolverAndReturnsDataWhenBoundMutationIsInvoked()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub(author : ""other"") {
                    content
                }
            }");

            dynamic receivedData = null;

            this.schema.OnSubscriptionMessageReceived += async (dynamic data) =>
            {
                await Task.Yield();
                receivedData = data;
            };

            this.schema.Execute(@"mutation mutate {
                testMutation {
                    content
                }
            }");

            Assert.AreEqual("test", receivedData.testSub.content);
        }

        [Test]
        public void ShouldInvokeOnSubscriptionMessageReceivedOnlyOnceAfterMutationIsPerformed()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub(author : ""other"") {
                    content
                }
            }");

            var counter = 0;

            this.schema.OnSubscriptionMessageReceived += async (dynamic data) =>
            {
                await Task.Yield();
                counter++;
            };

            Assert.AreEqual(0, counter);

            this.schema.Execute(@"mutation mutate {
                testMutation {
                    content
                }
            }");

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void DoesNotInvokeSubscriptionWhenMutationIsOnDifferentChannel()
        {
            var result = this.schema.Execute(@"subscription wasub {
                testSub(author : ""other"") {
                    content
                }
            }");

            var counter = 0;

            this.schema.OnSubscriptionMessageReceived += async (dynamic data) =>
            {
                await Task.Yield();
                counter++;
            };

            this.schema.Execute(@"mutation mutate {
                testMutation1 {
                    content
                }
            }");

            Assert.AreEqual(0, counter);
        }
    }
}
