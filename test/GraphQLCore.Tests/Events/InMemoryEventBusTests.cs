using GraphQLCore.Events;
using GraphQLCore.Language;
using GraphQLCore.Language.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQLCore.Tests.Events
{
    [TestFixture]
    public class InMemoryEventBusTests
    {
        private InMemoryEventBus eventBus;
        private GraphQLDocument operation;

        public class Message
        {
            public string Author { get; set; }
            public string Content { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            this.eventBus = new InMemoryEventBus();
            this.operation = new Parser(new Lexer()).Parse(new Source(@"
                subscription testSub {
                    newMessages(author : ""Bob"") {
                        content
                    }
                }
            "));
        }

        [Test]
        public async Task ShouldReceiveDataDefinedBySubscription()
        {
            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                0,
                null,
                new { },
                e => e.Author == "Bob",
                operation));

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();

                eventArgs = args;
            };

            await this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.AreEqual("testChannel", eventArgs.Channel);
        }

        [Test]
        public async Task ShouldNotReceiveAnythingIfNoSubscriptionIsDefined()
        {
            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();

                eventArgs = args;
            };

            await this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.IsNull(eventArgs);
        }

        [Test]
        public async Task DoesntSendDataWhenNoSubscriptionMatchesTheFilter()
        {
            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                0,
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();

                eventArgs = args;
            };

            await this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.IsNull(eventArgs);
        }

        [Test]
        public async Task InvokesOnMessageReceivedMultipleTimesIfMultipleSubscribersExists()
        {
            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                0,
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someAnotherClientId",
                0,
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            List<string> clientIds = new List<string>();

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();

                clientIds.Add(args.ClientId);
            };

            await this.eventBus.Publish(new Message() { Author = "Sam", Content = "stuff" }, "testChannel");


            Assert.AreEqual(2, clientIds.Count);
        }

        [Test]
        public async Task IgnoresMultipleSameSubscriptions()
        {
            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                0,
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            await this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                0,
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            List<string> clientIds = new List<string>();

            this.eventBus.OnMessageReceived += async (OnMessageReceivedEventArgs args) =>
            {
                await Task.Yield();
                clientIds.Add(args.ClientId);
            };

            await this.eventBus.Publish(new Message() { Author = "Sam", Content = "stuff" }, "testChannel");


            Assert.AreEqual(1, clientIds.Count);
        }

    }
}
