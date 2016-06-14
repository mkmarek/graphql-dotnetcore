namespace GraphQL.Tests.Type
{
    using GraphQL.Type;
    using GraphQL.Type.Scalars;
    using NUnit.Framework;
    public class GraphQLObjectTypeTests
    {
        private GraphQLObjectType<TestModel> type;

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Test", type.Name);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        [Test]
        public void Description_HasCorrectDescription()
        {
            Assert.AreEqual("Test description", type.Description);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLObjectType<TestModel>("Test", "Test description");
        }

        public class TestModel
        {
            public int Test { get; set; }
        }
    }
}
