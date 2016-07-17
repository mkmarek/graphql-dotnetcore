namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using NSubstitute;
    using NUnit.Framework;
    using Schemas;

    [TestFixture]
    public class TypeTranslatorTests
    {
        private ISchemaObserver schemaObserver;
        private TypeTranslator translator;

        [Test]
        public void GetType_BooleanType_CreatesGraphQLBooleanInstance()
        {
            var type = typeof(bool);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLBoolean>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void GetType_ClassType_UsesSchemaObserverToGetCorrectType()
        {
            var type = typeof(TestClass);

            var graphqlType = translator.GetType(type);

            this.schemaObserver.Received().GetSchemaTypeFor(type);
        }

        [Test]
        public void GetType_EnumType_ReturnsNonNullOfEnumSchemeType()
        {
            var type = typeof(FurColor);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<FurColorEnum>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void GetType_FloatType_CreatesGraphQLFloatInstance()
        {
            var type = typeof(float);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLFloat>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void GetType_IntNamedType_GetsValueFromKnownTypesByCorrectName()
        {
            this.schemaObserver.GetOutputKnownTypes().Returns(new GraphQLBaseType[] { new GraphQLInt() });

            var graphqlType = translator.GetType(GetGraphQLNamedType("Int"));

            Assert.IsInstanceOf<GraphQLInt>(graphqlType);
        }

        [Test]
        public void GetType_IntType_CreatesGraphQLIntInstance()
        {
            var type = typeof(int);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLInt>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void GetType_ListOfNonNullInt_CreatesGraphQLListInstance()
        {
            var type = typeof(int[]);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLList>(graphqlType);
            Assert.IsInstanceOf<GraphQLNonNullType>(((GraphQLList)graphqlType).MemberType);
            Assert.IsInstanceOf<GraphQLInt>(((GraphQLNonNullType)((GraphQLList)graphqlType).MemberType).UnderlyingNullableType);
        }

        [Test]
        public void GetType_NullableBooleanType_CreatesGraphQLBooleanInstance()
        {
            var type = typeof(bool?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLBoolean>(graphqlType);
        }

        [Test]
        public void GetType_NullableEnumType_ReturnsEnumSchemeType()
        {
            var type = typeof(FurColor?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<FurColorEnum>(graphqlType);
        }

        [Test]
        public void GetType_NullableFloatType_CreatesGraphQLFloatInstance()
        {
            var type = typeof(float?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLFloat>(graphqlType);
        }

        [Test]
        public void GetType_NullableIntType_CreatesGraphQLIntInstance()
        {
            var type = typeof(int?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLInt>(graphqlType);
        }

        [Test]
        public void GetType_NullableStringType_CreatesGraphQLStringInstance()
        {
            var type = typeof(string);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLString>(graphqlType);
        }

        [Test]
        public void GetType_NullableStructType_ReturnsSchemeObjectType()
        {
            var type = typeof(TestStruct?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<TestScructObject>(graphqlType);
        }

        [Test]
        public void GetType_StructType_ReturnsNonNullOfObjectType()
        {
            var type = typeof(TestStruct);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<TestScructObject>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaObserver = Substitute.For<ISchemaObserver>();
            this.schemaObserver.GetSchemaTypeFor(typeof(TestStruct)).Returns(new TestScructObject());
            this.schemaObserver.GetSchemaTypeFor(typeof(FurColor)).Returns(new FurColorEnum());
            this.translator = new TypeTranslator(this.schemaObserver);
        }

        private static GraphQLCore.Language.AST.GraphQLNamedType GetGraphQLNamedType(string name)
        {
            return new GraphQLCore.Language.AST.GraphQLNamedType()
            {
                Name = new GraphQLCore.Language.AST.GraphQLName() { Value = name }
            };
        }

        public struct TestStruct { }

        public class TestClass { }

        public class TestScructObject : GraphQLObjectType<TestStruct>
        {
            public TestScructObject() : base("TestStruct", "")
            {
            }
        }
    }
}