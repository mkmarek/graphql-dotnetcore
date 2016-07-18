namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using Schemas;

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

            Assert.IsInstanceOf<FurColorEnum>(objectType);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaRepository = new SchemaRepository();
        }
    }
}