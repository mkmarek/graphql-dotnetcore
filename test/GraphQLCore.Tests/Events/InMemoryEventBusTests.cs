using GraphQLCore.Events;
using GraphQLCore.Language;
using GraphQLCore.Language.AST;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public void ShouldReceiveDataDefinedBySubscription()
        {
            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                null,
                new { },
                e => e.Author == "Bob",
                operation));

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += (OnMessageReceivedEventArgs args) =>
            {
                eventArgs = args;
            };

            this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.AreEqual("testChannel", eventArgs.Channel);
        }

        [Test]
        public void ShouldNotReceiveAnythingIfNoSubscriptionIsDefined()
        {
            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += (OnMessageReceivedEventArgs args) =>
            {
                eventArgs = args;
            };

            this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.IsNull(eventArgs);
        }

        [Test]
        public void DoesntSendDataWhenNoSubscriptionMatchesTheFilter()
        {
            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            OnMessageReceivedEventArgs eventArgs = null;

            this.eventBus.OnMessageReceived += (OnMessageReceivedEventArgs args) =>
            {
                eventArgs = args;
            };

            this.eventBus.Publish(new Message() { Author = "Bob", Content = "stuff" }, "testChannel");


            Assert.IsNull(eventArgs);
        }

        [Test]
        public void InvokesOnMessageReceivedMultipleTimesIfMultipleSubscribersExists()
        {
            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someAnotherClientId",
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            List<string> clientIds = new List<string>();

            this.eventBus.OnMessageReceived += (OnMessageReceivedEventArgs args) =>
            {
                clientIds.Add(args.ClientId);
            };

            this.eventBus.Publish(new Message() { Author = "Sam", Content = "stuff" }, "testChannel");


            Assert.AreEqual(2, clientIds.Count);
        }

        [Test]
        public void IgnoresMultipleSameSubscriptions()
        {
            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            this.eventBus.Subscribe(EventBusSubscription.Create<Message>(
                "testChannel",
                "someClientId",
                null,
                new { },
                e => e.Author == "Sam",
                operation));

            List<string> clientIds = new List<string>();

            this.eventBus.OnMessageReceived += (OnMessageReceivedEventArgs args) =>
            {
                clientIds.Add(args.ClientId);
            };

            this.eventBus.Publish(new Message() { Author = "Sam", Content = "stuff" }, "testChannel");


            Assert.AreEqual(1, clientIds.Count);
        }

    }
}
