namespace GraphQLCore.Tests.Type
{
    using Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;

    public class GraphQLObjectTypeTests
    {
        private GraphQLObjectType<TestModel> type;

        [Test]
        public void AddField_OneResolverAndOnveAcessorWithSameNames_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                type.Field("A", () => "x");
                type.Field("A", model => model.Test);
            }));

            Assert.AreEqual("Can't insert two fields with the same name.", exception.Message);
        }

        [Test]
        public void AddField_TwoAcessorsWithSameNames_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                type.Field("A", model => model.Test);
                type.Field("A", model => model.Test);
            }));

            Assert.AreEqual("Can't insert two fields with the same name.", exception.Message);
        }

        [Test]
        public void AddField_TwoResolversWithSameNames_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                type.Field("A", () => "x");
                type.Field("A", () => "y");
            }));

            Assert.AreEqual("Can't insert two fields with the same name.", exception.Message);
        }

        [Test]
        public void Description_HasCorrectDescription()
        {
            Assert.AreEqual("Test description", type.Description);
        }

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Test", type.Name);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLTestModelType(new GraphQLSchema());
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        public class GraphQLTestModelType : GraphQLObjectType<TestModel>
        {
            public GraphQLTestModelType(GraphQLSchema schema) : base("Test", "Test description", schema)
            {
            }
        }

        public class TestModel
        {
            public int Test { get; set; }
        }
    }
}