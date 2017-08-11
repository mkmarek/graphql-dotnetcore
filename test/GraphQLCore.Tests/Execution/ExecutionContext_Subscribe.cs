namespace GraphQLCore.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using GraphQLCore.Events;
    using GraphQLCore.Execution;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Complex;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Validation;

    [TestFixture]
    public class ExecutionContext_Subscribe
    {
        private GraphQLSchema schema;
        private Inbox data;

        [Test]
        public void SimpleSubscribe_ReturnsOneResult()
        {
            var result = this.Subscribe(@"{
                __typename
            }");

            Assert.AreEqual("Query", result.Single().Data.__typename);
            Assert.IsNull(result.Single().Errors);
        }

        [Test]
        public void SubscribeWithErrors_ReturnsErrors()
        {
            var result = this.Subscribe(@"{
                unknown
            }");

            Assert.AreEqual(null, result.Single().Data);
            ErrorAssert.AreEqual(
                "Cannot query field \"unknown\" on type \"Query\".",
                result.Single().Errors.Single(), 2, 17);
        }

        [Test]
        public void SubscribeWithResolverError_ReturnsResult()
        {
            var result = this.Subscribe(@"{
                nested {
                    __typename
                    thisIsError : error
                }
            }");

            Assert.AreEqual("Query", result.Single().Data.nested.__typename);
            Assert.IsNull(result.Single().Data.nested.thisIsError);

            ErrorAssert.AreEqual(
                "error",
                result.Single().Errors.Single(),
                4, 21,
                new[] { "nested", "thisIsError" });
        }

        [Test]
        public async Task SubscribeSubscription_ReturnsMultipleResults()
        {
            var result = this.SubscribeToEmails();
            
            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject",
                Message = "message",
                Unread = true
            });

            var singleResult = result.Single();
            Assert.AreEqual("foo@bar.org", singleResult.Data.importantEmail.email.from);
            Assert.AreEqual("subject", singleResult.Data.importantEmail.email.subject);
            Assert.AreEqual(1, singleResult.Data.importantEmail.inbox.unread);
            Assert.AreEqual(2, singleResult.Data.importantEmail.inbox.total);
            Assert.IsNull(singleResult.Errors);
        }

        [Test]
        public void ProducesPayloadForMultipleSubscribeInSameSubscription()
        {
            var result1 = this.SubscribeToEmails();
            var result2 = this.SubscribeToEmails();

            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject",
                Message = "message",
                Unread = true
            });

            var singleResult = result1.Single();
            
            Assert.AreEqual(JsonConvert.SerializeObject(singleResult), JsonConvert.SerializeObject(result2.Single()));
            Assert.AreEqual("foo@bar.org", singleResult.Data.importantEmail.email.from);
            Assert.AreEqual("subject", singleResult.Data.importantEmail.email.subject);
            Assert.AreEqual(1, singleResult.Data.importantEmail.inbox.unread);
            Assert.AreEqual(2, singleResult.Data.importantEmail.inbox.total);
            Assert.IsNull(singleResult.Errors);
        }

        [Test]
        public void ProducesAPayloadPerSubscriptionEvent()
        {
            var result = this.SubscribeToEmails();

            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject",
                Message = "message",
                Unread = true
            });

            var singleResult = result.Single();
            Assert.AreEqual("foo@bar.org", singleResult.Data.importantEmail.email.from);
            Assert.AreEqual("subject", singleResult.Data.importantEmail.email.subject);
            Assert.AreEqual(1, singleResult.Data.importantEmail.inbox.unread);
            Assert.AreEqual(2, singleResult.Data.importantEmail.inbox.total);
            Assert.IsNull(singleResult.Errors);

            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject2",
                Message = "message2",
                Unread = true
            });

            Assert.AreEqual(2, result.Count);
            var anotherResult = result[1];
            Assert.AreEqual("foo@bar.org", anotherResult.Data.importantEmail.email.from);
            Assert.AreEqual("subject2", anotherResult.Data.importantEmail.email.subject);
            Assert.AreEqual(2, anotherResult.Data.importantEmail.inbox.unread);
            Assert.AreEqual(3, anotherResult.Data.importantEmail.inbox.total);
            Assert.IsNull(anotherResult.Errors);
        }

        [Test]
        public void EventsOrderIsCorrectWhenMultipleTriggeredTogether()
        {
            var result = this.SubscribeToEmails();

            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject",
                Message = "message",
                Unread = true
            });

            this.SendImportantEmail(new Email()
            {
                From = "foo@bar.org",
                Subject = "subject2",
                Message = "message2",
                Unread = true
            });

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("foo@bar.org", result[0].Data.importantEmail.email.from);
            Assert.AreEqual("subject", result[0].Data.importantEmail.email.subject);
            Assert.AreEqual(1, result[0].Data.importantEmail.inbox.unread);
            Assert.AreEqual(2, result[0].Data.importantEmail.inbox.total);
            Assert.IsNull(result[0].Errors);

            Assert.AreEqual("foo@bar.org", result[1].Data.importantEmail.email.from);
            Assert.AreEqual("subject2", result[1].Data.importantEmail.email.subject);
            Assert.AreEqual(2, result[1].Data.importantEmail.inbox.unread);
            Assert.AreEqual(3, result[1].Data.importantEmail.inbox.total);
            Assert.IsNull(result[1].Errors);
        }

        [Test]
        public void SubscriptionError_IsReported()
        {
            var result = this.Subscribe(@"
            subscription {
                importantEmail {
                    undefined
                }
            }
            ");

            var error = result.Single().Errors.Single();
            ErrorAssert.AreEqual("Cannot query field \"undefined\" on type \"EmailEvent\".",
                error, 4, 21);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new EmailSchema();
        }

        private IList<ExecutionResult> Subscribe(string query)
        {
            return this.schema.Subscribe(query).ToList().GetAwaiter().GetResult();
        }

        private IList<ExecutionResult> SubscribeAsync(string query)
        {
            var result = new List<ExecutionResult>();
            this.schema.Subscribe(query, null, null, Guid.NewGuid().ToString(), "1")
                .Subscribe(result.Add);

            return result;
        }

        private IList<ExecutionResult> SubscribeToEmails()
        {
            return this.SubscribeAsync(@"
            subscription ($priority: Int = 0) {
                importantEmail(priority: $priority) {
                    email {
                        from
                        subject
                    }
                    inbox {
                        unread
                        total
                    }
                }
            }");
        }

        private void SendImportantEmail(Email newEmail)
        {
            this.schema.Execute(@"
            mutation {
                sendImportantEmail(email: " + newEmail + @") {
                    __typename
                }
            }");
        }
 
        private class Email
        {
            [JsonProperty("from")]
            public string From { get; set; }

            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("unread")]
            public bool? Unread { get; set; }

            public override string ToString()
            {
                var serializer = new JsonSerializer();
                var stringWriter = new StringWriter();
                using (var writer = new JsonTextWriter(stringWriter))
                {
                    writer.QuoteName = false;
                    serializer.Serialize(writer, this);
                }
                return stringWriter.ToString();
            }
        }

        private class EmailInputType : GraphQLInputObjectType<Email>
        {
            public EmailInputType() : base("EmailInput", null)
            {
                this.Field("from", e => e.From);
                this.Field("subject", e => e.Subject);
                this.Field("message", e => e.Message);
                this.Field("unread", e => e.Unread);
            }
        }

        private class EmailType : GraphQLObjectType<Email>
        {
            public EmailType() : base("Email", null)
            {
                this.Field("from", e => e.From);
                this.Field("subject", e => e.Subject);
                this.Field("message", e => e.Message);
                this.Field("unread", e => e.Unread);
            }
        }

        private class Inbox
        {
            public ICollection<Email> Emails { get; set; }
        }

        private class InboxType : GraphQLObjectType<Inbox>
        {
            public InboxType() : base("Inbox", null)
            {
                this.Field("total", e => e.Emails.Count as int?);
                this.Field("unread", e => e.Emails.Count(email => email.Unread.GetValueOrDefault()));
                this.Field("emails", e => e.Emails);
            }
        }

        private class QueryType : GraphQLObjectType
        {
            public QueryType() : base("Query", null)
            {
                this.Field("inbox", () => new Inbox());
                this.Field("error", () => this.Throw());
                this.Field("nested", () => this);
            }

            private string Throw()
            {
                throw new Exception("error");
            }
        }

        private class MutationType : GraphQLObjectType
        {
            private Inbox inbox = new Inbox()
            {
                Emails = new List<Email>()
                {
                    new Email()
                    {
                        From = "joe@graphql.org",
                        Subject = "Hello",
                        Message = "Hello world",
                        Unread = false
                    }
                }
            };

            public MutationType() : base("Mutation", null)
            {
                this.Field("sendImportantEmail", (Email email) => this.SendImportantEmail(email))
                    .OnChannel("importantEmail");
            }

            private EmailEvent SendImportantEmail(Email email)
            {
                this.inbox.Emails.Add(email);
                return new EmailEvent()
                {
                    Email = email,
                    Inbox = this.inbox
                };
            }
        }

        private class EmailEvent
        {
            public Email Email { get; set; }
            public Inbox Inbox { get; set; }
        }

        private class EmailEventType : GraphQLObjectType<EmailEvent>
        {
            public EmailEventType() : base("EmailEvent", null)
            {
                this.Field("email", e => e.Email);
                this.Field("inbox", e => e.Inbox);
            }
        }

        private class SubscriptionType : GraphQLSubscriptionType
        {
            public SubscriptionType() : base("Subscription", null, new InMemoryEventBus())
            {
                this.Field("importantEmail", (IContext<EmailEvent> ctx, int? priority) => ctx.Instance)
                    .OnChannel("importantEmail");
            }
        }

        private class EmailSchema : GraphQLSchema
        {
            public EmailSchema()
            {
                var query = new QueryType();
                var mutation = new MutationType();
                var subscription = new SubscriptionType();

                this.AddKnownType(new EmailType());
                this.AddKnownType(new EmailInputType());
                this.AddKnownType(new InboxType());
                this.AddKnownType(new EmailEventType());
                this.AddKnownType(query);
                this.AddKnownType(mutation);
                this.AddKnownType(subscription);

                this.Query(query);
                this.Mutation(mutation);
                this.Subscription(subscription);
            }
        }
    }
}
