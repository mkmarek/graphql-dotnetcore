namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using Schemas;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class SchemaRepositoryTests
    {
        private SchemaRepository schemaRepository;

        [Test]
        public void GetSchemaTypeFor_Class_ReturnsCorrectSchemaType()
        {
            this.schemaRepository.AddKnownType(new ComplicatedObjectType());

            var objectType = this.schemaRepository.GetSchemaTypeFor(typeof(ComplicatedObject));

            Assert.IsInstanceOf<ComplicatedObjectType>(objectType);
        }

        [Test]
        public void GetSchemaTypeFor_Enum_ReturnsCorrectSchemaType()
        {
            this.schemaRepository.AddKnownType(new FurColorEnum());

            var objectType = this.schemaRepository.GetSchemaTypeFor(typeof(FurColor));

            Assert.IsInstanceOf<GraphQLNonNullType>(objectType);
            Assert.IsInstanceOf<FurColorEnum>(((GraphQLNonNullType)objectType).UnderlyingNullableType);
        }

        [Test]
        public void GetSchemaTypeFor_NullableEnum_ReturnsCorrectSchemaType()
        {
            this.schemaRepository.AddKnownType(new FurColorEnum());

            var objectType = this.schemaRepository.GetSchemaTypeFor(typeof(FurColor?));

            Assert.IsInstanceOf<FurColorEnum>(objectType);
        }

        [Test]
        public void GetSchemaTypeFor_JaggedArray_ReturnsCorrectSchemaType()
        {
            var objectType = this.schemaRepository.GetSchemaTypeFor(typeof(string[][][]));

            Assert.AreEqual("[[[String]]]", objectType.ToString());
        }

        [Test]
        public void GetSchemaTypeFor_ComplicatedCollection_ReturnsCorrectSchemaType()
        {
            this.schemaRepository.AddKnownType(new ComplicatedObjectType());

            var objectType = this.schemaRepository.GetSchemaTypeFor(typeof(IEnumerable<List<ComplicatedObject[][][]>>));

            Assert.AreEqual("[[[[[ComplicatedObjectType]]]]]", objectType.ToString());
        }

        [Test]
        public void GetInputSchemaTypeFor_JaggedArray_ReturnsCorrectSchemaType()
        {
            var objectType = this.schemaRepository.GetSchemaInputTypeFor(typeof(string[][][][]));

            Assert.AreEqual("[[[[String]]]]", objectType.ToString());
        }

        [Test]
        public void GetInputSystemTypeFor_ScalarType_ReturnsCorrectSystemType()
        {
            var inputType = this.schemaRepository.GetInputSystemTypeFor(new GraphQLString());

            Assert.AreEqual(typeof(string), inputType);
        }

        [Test]
        public void GetInputSystemTypeFor_ComplexType_ReturnsCorrectSystemType()
        {
            this.schemaRepository.AddKnownType(new ComplicatedInputObjectType());

            var inputType = this.schemaRepository.GetInputSystemTypeFor(new ComplicatedInputObjectType());

            Assert.AreEqual(typeof(ComplicatedObject), inputType);
        }

        [Test]
        public void GetInputSystemTypeFor_Collection_ReturnsCorrectSystemType()
        {
            var inputType = this.schemaRepository.GetInputSystemTypeFor(new GraphQLList(new GraphQLString()));

            Assert.AreEqual(typeof(string), inputType.GenericTypeArguments.Single());
        }

        [Test]
        public void GetInputSystemTypeFor_NestedList_ReturnsCorrectSystemType()
        {
            var inputType = this.schemaRepository.GetInputSystemTypeFor(new GraphQLList(new GraphQLList(new GraphQLString())));

            Assert.AreEqual(typeof(string), inputType.GenericTypeArguments.Single().GenericTypeArguments.Single());
        }

        [Test]
        public void GetInputSystemTypeFor_NonNullEnum_ReturnsCorrectSystemType()
        {
            this.schemaRepository.AddKnownType(new FurColorEnum());

            var inputType = this.schemaRepository.GetInputSystemTypeFor(new GraphQLNonNullType(new FurColorEnum()));

            Assert.AreEqual(typeof(FurColor), inputType);
        }

        [Test]
        public void GetInputSystemTypeFor_NullableEnum_ReturnsCorrectSystemType()
        {
            this.schemaRepository.AddKnownType(new FurColorEnum());

            var inputType = this.schemaRepository.GetInputSystemTypeFor(new FurColorEnum());

            Assert.AreEqual(typeof(FurColor?), inputType);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaRepository = new SchemaRepository();
        }
    }
}