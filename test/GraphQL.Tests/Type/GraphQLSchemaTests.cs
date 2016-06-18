namespace GraphQL.Tests.Type
{
    using Exceptions;
    using GraphQL.Type;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class GraphQLSchemaTests
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_MultipleOperationsNoOperationName_ThrowsAnError()
        {
            var exception = Assert.Throws<Exception>(new TestDelegate(() =>
                this.schema.Execute("query Example { hello } query OtherExample { hello }")));

            Assert.AreEqual("Must provide operation name if query contains multiple operations.", 
                exception.Message);
        }

       

        [Test]
        public void Execute_GenericObjectWithoutResolver_ThrowsException()
        {
            var rootType = new GraphQLObjectType<TestType>("RootQueryType", "");
            this.schema = new GraphQLSchema(rootType);
            rootType.AddField("hello", o => o.Hello);

            var exception = Assert.Throws<GraphQLException>(
                new TestDelegate(() => this.schema.Execute("{ hello }")));

            Assert.AreEqual("GraphQLObjectType RootQueryType doesn't have a resolver", exception.Message);
        }


        [SetUp]
        public void SetUp()
        {
            var rootType = new GraphQLObjectType("RootQueryType", "");
            rootType.AddField("hello", () => "world");
            rootType.AddField("test",  () => "test");

            this.schema = new GraphQLSchema(rootType);
        }

        public class TestType
        {
            public string Hello { get; set; }
            public string Test { get; set; }
        }
    }
}
