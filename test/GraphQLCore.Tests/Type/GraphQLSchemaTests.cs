namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

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

            Assert.IsNotNull(result.data.a);
        }

        [Test]
        public void SingleOperationOperationNameProvided_PicksOperationByOperationName()
        {
            var result = this.schema.Execute(this.singleOperationQuery, new ExpandoObject(), "q1");

            Assert.IsNotNull(result.data.a);
        }

        [Test]
        public void NotExistingOperationNameProvided_TrowsException()
        {
            var result = this.schema.Execute(this.singleOperationQuery, new ExpandoObject(), "q2");
            var errors = (IList<GraphQLException>)result.errors;

            Assert.AreEqual("Unknown operation named \"q2\".", errors.Single().Message);
        }

        [Test]
        public void MultipleOperationsNoOperationNameProvided_TrowsException()
        {
            var result = this.schema.Execute(this.multipleOperationQuery);
            var errors = (IList<GraphQLException>)result.errors;

            Assert.AreEqual("Must provide operation name if query contains multiple operations.", errors.Single().Message);
        }

        [Test]
        public void MultipleOperationsSelectingFirstOperation_ReturnsResultFromTheFirstOperation()
        {
            var result = this.schema.Execute(this.multipleOperationQuery, new ExpandoObject(), "q1");

            Assert.IsNotNull(result.data.a);
        }

        [Test]
        public void MultipleOperationsSelectingSecondOperation_ReturnsResultFromTheSecondOperation()
        {
            var result = this.schema.Execute(this.multipleOperationQuery, new ExpandoObject(), "q2");

            Assert.IsNotNull(result.data.b);
        }

        [Test]
        public void NoOperationProvided_ThrowsError()
        {
            var result = this.schema.Execute("");
            var errors = (IList<GraphQLException>)result.errors;

            Assert.AreEqual("Must provide an operation.", errors.Single().Message);
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