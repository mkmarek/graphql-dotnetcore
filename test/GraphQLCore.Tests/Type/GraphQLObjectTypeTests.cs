namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Exceptions;
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

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        [Test]
        public void Field_CanHaveDescription()
        {
            this.type.Field("test", e => e.Test).WithDescription("description");

            Assert.AreEqual("description", this.type.GetFieldInfo("test").Description);
        }

        [Test]
        public void Field_CanBeDeprecated()
        {
            this.type.Field("test", e => e.Test).IsDeprecated("because");

            var fieldInfo = this.type.GetFieldInfo("test");

            Assert.AreEqual(true, fieldInfo.IsDeprecated);
            Assert.AreEqual("because", fieldInfo.DeprecationReason);
        }

        [Test]
        public void Field_CanSetArgumentsDefaultValues()
        {
            this.type.Field("test", (int? a, int? b, int c) => a + b + c)
                .WithDefaultValue("a", 4)
                .WithDefaultValue("b", 3);

            var fieldInfo = this.type.GetFieldInfo("test");

            Assert.AreEqual(4, fieldInfo.Arguments["a"].DefaultValue.Value);
            Assert.AreEqual(3, fieldInfo.Arguments["b"].DefaultValue.Value);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLTestModelType();
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