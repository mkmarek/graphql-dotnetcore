namespace GraphQLCore.Tests.Type
{
    using Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System;
    using System.Dynamic;

    [TestFixture]
    public class GraphQLSchemaTests
    {
        private GraphQLSchema schema;
        private string singleOperationQuery;
        private string multipleOperationQuery;


        [Test]
        public void SingleOperationNoOperationNameProvided_PicksSingleOne()
        {
            var result = this.schema.Execute(this.singleOperationQuery);

            Assert.IsNotNull(result.a);
        }

        [Test]
        public void SingleOperationOperationNameProvided_PicksOperationByOperationName()
        {
            var result = this.schema.Execute(this.singleOperationQuery, new ExpandoObject(), "q1");

            Assert.IsNotNull(result.a);
        }

        [Test]
        public void NotExistingOperationNameProvided_TrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(() =>
            {
                var result = this.schema.Execute(this.singleOperationQuery, new ExpandoObject(), "q2");
            });

            Assert.AreEqual("Unknown operation named \"q2\".", exception.Message);
        }

        [Test]
        public void MultipleOperationsNoOperationNameProvided_TrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(() =>
            {
                var result = this.schema.Execute(this.multipleOperationQuery);
            });

            Assert.AreEqual("Must provide operation name if query contains multiple operations.", exception.Message);
        }

        [Test]
        public void MultipleOperationsSelectingFirstOperation_ReturnsResultFromTheFirstOperation()
        {
            var result = this.schema.Execute(this.multipleOperationQuery, new ExpandoObject(), "q1");

            Assert.IsNotNull(result.a);
        }

        [Test]
        public void MultipleOperationsSelectingSecondOperation_ReturnsResultFromTheSecondOperation()
        {
            var result = this.schema.Execute(this.multipleOperationQuery, new ExpandoObject(), "q2");

            Assert.IsNotNull(result.b);
        }

        [Test]
        public void NoOperationProvided_ThrowsError()
        {
            var exception = Assert.Throws<GraphQLException>(() =>
            {
                var result = this.schema.Execute("");
            });

            Assert.AreEqual("Must provide an operation.", exception.Message);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new RootQueryType();

            schema.Query(rootType);

            this.singleOperationQuery = "query q1 { a : hello }";
            this.multipleOperationQuery = "query q1 { a : hello } query q2 { b : hello }";
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType() : base("RootQueryType", "")
            {
                this.Field("hello", () => "world");
            }
        }
    }
}