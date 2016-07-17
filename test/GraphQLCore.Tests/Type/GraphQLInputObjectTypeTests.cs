namespace GraphQLCore.Tests.Type
{
    using Exceptions;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    public class GraphQLInputObjectTypeTests
    {
        private GraphQLInputObjectType<TestModel> type;

        public interface TestInterfaceModel
        {
            int Test { get; set; }
        }

        [Test]
        public void AddField_CollectionOfInterfacesTypeField_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                type.Field("nestedInterfaceCollection", model => model.NestedInterfaceCollection);
            }));

            Assert.AreEqual("Can't set accessor to interface based field", exception.Message);
        }

        [Test]
        public void AddField_CollectionOfNonInterfacesTypeField_DoesntThrowException()
        {
            type.Field("nestedCollection", model => model.NestedCollection);
        }

        [Test]
        public void AddField_InterfaceTypeField_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() =>
            {
                type.Field("nested", model => model.Nested);
            }));

            Assert.AreEqual("Can't set accessor to interface based field", exception.Message);
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
            this.type = new GraphQLTestInputModelType();
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        public class GraphQLTestInputModelType : GraphQLInputObjectType<TestModel>
        {
            public GraphQLTestInputModelType() : base("Test", "Test description")
            {
            }
        }

        public class TestModel
        {
            public TestInterfaceModel Nested { get; set; }
            public IEnumerable<string> NestedCollection { get; set; }
            public TestInterfaceModel[] NestedInterfaceCollection { get; set; }
            public int Test { get; set; }
        }
    }
}