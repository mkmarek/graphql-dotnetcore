namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalars;
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
        public void GetTypeObserverFor_ObjectType_ReturnsCorrectObserver()
        {
            this.schemaObserver.AddKnownType(new FurColorEnum());

            var typeObserver = this.translator.GetObjectTypeTranslatorFor(typeof(FurColor));

            Assert.IsNotNull(typeObserver);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaObserver = Substitute.For<ISchemaObserver>();
            this.schemaObserver.GetSchemaTypeFor(typeof(TestStruct)).Returns(new TestScructObject());
            this.schemaObserver.GetSchemaTypeFor(typeof(FurColor)).Returns(new FurColorEnum());
            this.translator = new TypeTranslator(this.schemaObserver);
        }

        [Test]
        public void Translate_ClassType_UsesSchemaObserverToGetCorrectType()
        {
            var type = typeof(TestClass);

            var graphqlType = translator.GetType(type);

            this.schemaObserver.Received().GetSchemaTypeFor(type);
        }

        [Test]
        public void Translate_EnumType_ReturnsNonNullOfEnumSchemeType()
        {
            var type = typeof(FurColor);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<FurColorEnum>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void Translate_NullableEnumType_ReturnsEnumSchemeType()
        {
            var type = typeof(FurColor?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<FurColorEnum>(graphqlType);
        }

        [Test]
        public void Translate_NullableStructType_ReturnsSchemeObjectType()
        {
            var type = typeof(TestStruct?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<TestScructObject>(graphqlType);
        }

        [Test]
        public void Translate_StructType_ReturnsNonNullOfObjectType()
        {
            var type = typeof(TestStruct);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<TestScructObject>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void TranslateList_ListOfNonNullInt_CreatesGraphQLListInstance()
        {
            var type = typeof(int[]);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLList>(graphqlType);
            Assert.IsInstanceOf<GraphQLNonNullType>(((GraphQLList)graphqlType).MemberType);
            Assert.IsInstanceOf<GraphQLInt>(((GraphQLNonNullType)((GraphQLList)graphqlType).MemberType).UnderlyingNullableType);
        }

        [Test]
        public void TranslateScalar_BooleanType_CreatesGraphQLBooleanInstance()
        {
            var type = typeof(bool);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLBoolean>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void TranslateScalar_FloatType_CreatesGraphQLFloatInstance()
        {
            var type = typeof(float);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLFloat>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void TranslateScalar_IntType_CreatesGraphQLIntInstance()
        {
            var type = typeof(int);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLNonNullType>(graphqlType);
            Assert.IsInstanceOf<GraphQLInt>(((GraphQLNonNullType)graphqlType).UnderlyingNullableType);
        }

        [Test]
        public void TranslateScalar_NullableBooleanType_CreatesGraphQLBooleanInstance()
        {
            var type = typeof(bool?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLBoolean>(graphqlType);
        }

        [Test]
        public void TranslateScalar_NullableFloatType_CreatesGraphQLFloatInstance()
        {
            var type = typeof(float?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLFloat>(graphqlType);
        }

        [Test]
        public void TranslateScalar_NullableIntType_CreatesGraphQLIntInstance()
        {
            var type = typeof(int?);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLInt>(graphqlType);
        }

        [Test]
        public void TranslateScalar_NullableStringType_CreatesGraphQLStringInstance()
        {
            var type = typeof(string);

            var graphqlType = translator.GetType(type);

            Assert.IsInstanceOf<GraphQLString>(graphqlType);
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