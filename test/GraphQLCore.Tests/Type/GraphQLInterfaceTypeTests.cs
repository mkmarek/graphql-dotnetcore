namespace GraphQLCore.Tests.Type
{
    using Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Linq;

    public class GraphQLInterfaceTypeTests
    {
        private GraphQLInterfaceType<TestInterfaceModel> type;

        public interface TestInterfaceModel
        {
            int Test { get; set; }
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
        public void CreateInterfaceType_ObjectGenericArgument_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                var invalidType = new GraphQLInvalidTestInterfaceModelType();
            }));

            Assert.AreEqual($"Type {typeof(TestModel).FullName} has to be an interface type", exception.Message);
        }

        [Test]
        public void Description_HasCorrectDescription()
        {
            Assert.AreEqual("Test description", type.Description);
        }

        [Test]
        public void GetFieldsInfo_SingleAccessor_ReturnsCorrectInformationReflectedFromLambda()
        {
            type.Field("A", e => e.Test);

            var info = type.GetFieldsInfo();

            Assert.AreEqual("A", info.Single().Name);
            Assert.AreEqual(typeof(int), info.Single().SystemType);
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
            this.type = new GraphQLTestInterfaceModelType();
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        public class GraphQLInvalidTestInterfaceModelType : GraphQLInterfaceType<TestModel>
        {
            public GraphQLInvalidTestInterfaceModelType() : base("Test", "Test description")
            {
            }
        }

        public class GraphQLTestInterfaceModelType : GraphQLInterfaceType<TestInterfaceModel>
        {
            public GraphQLTestInterfaceModelType() : base("Test", "Test description")
            {
            }
        }

        public class TestModel
        {
            public int Test { get; set; }
        }
    }
}