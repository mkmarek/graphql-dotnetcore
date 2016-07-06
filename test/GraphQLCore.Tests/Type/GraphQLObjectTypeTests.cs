namespace GraphQLCore.Tests.Type
{
    using Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Linq;

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
        public void GetFieldInfo_SingleResolverWithTwoArguments_ReturnsCorrectAmountOfArguments()
        {
            type.Field("A", (int a, string b) => "y");

            var info = type.GetFieldInfo("A");

            Assert.AreEqual(2, info.Arguments.Count);
        }

        [Test]
        public void GetFieldsInfo_SingleAccessor_ReturnsCorrectInformationReflectedFromLambda()
        {
            type.Field("A", e => e.Test);

            var info = type.GetFieldsInfo();

            Assert.AreEqual("A", info.Single().Name);
            Assert.AreEqual(typeof(int), info.Single().ReturnValueType);
            Assert.AreEqual(false, info.Single().IsResolver);
        }

        [Test]
        public void GetFieldsInfo_SingleAccessor_ReturnsZeroArguments()
        {
            type.Field("A", e => e.Test);

            var info = type.GetFieldsInfo();

            Assert.AreEqual(0, info.Single().Arguments.Count);
        }

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Test", type.Name);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLTestModelType();
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        public class GraphQLTestModelType : GraphQLObjectType<TestModel>
        {
            public GraphQLTestModelType() : base("Test", "Test description")
            {
            }
        }

        public class TestModel
        {
            public int Test { get; set; }
        }
    }
}