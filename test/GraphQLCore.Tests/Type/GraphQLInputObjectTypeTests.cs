namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using NSubstitute;
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
        public void GetFromAst_ReturnsCorrectType()
        {
            Assert.IsInstanceOf<TestModel>(this.type.GetFromAst(new GraphQLObjectValue(), null).Value);
        }

        [Test]
        public void GetFromAst_CanAssignScalarType()
        {
            var schemaRepository = Substitute.For<ISchemaRepository>();
            schemaRepository.GetSchemaInputTypeFor(typeof(int)).ReturnsForAnyArgs(new GraphQLString());

            this.type.Field("stringValue", e => e.StringValue);

            Assert.AreEqual("Foo", ((TestModel)this.type.GetFromAst(new GraphQLObjectValue() {
                Fields = new GraphQLObjectField[] {
                    new GraphQLObjectField()
                    {
                        Name = new GraphQLName() { Value = "stringValue" },
                        Value = new GraphQLScalarValue(ASTNodeKind.StringValue) { Value = "Foo" } }
                }
            }, schemaRepository).Value).StringValue);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Test", type.ToString());
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLTestInputModelType();
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
            public string StringValue { get; set; }
        }
    }
}