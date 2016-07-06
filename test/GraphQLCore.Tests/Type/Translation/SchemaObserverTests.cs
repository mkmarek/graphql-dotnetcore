namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using Schemas;

    [TestFixture]
    public class SchemaObserverTests
    {
        private SchemaObserver schemaObserver;

        [Test]
        public void GetSchemaTypeFor_Class_ReturnsCorrectSchemaType()
        {
            this.schemaObserver.AddKnownType(new ComplicatedObjectType());

            var objectType = this.schemaObserver.GetSchemaTypeFor(typeof(ComplicatedObject));

            Assert.IsInstanceOf<ComplicatedObjectType>(objectType);
        }

        [Test]
        public void GetSchemaTypeFor_Enum_ReturnsCorrectSchemaType()
        {
            this.schemaObserver.AddKnownType(new FurColorEnum());

            var objectType = this.schemaObserver.GetSchemaTypeFor(typeof(FurColor));

            Assert.IsInstanceOf<FurColorEnum>(objectType);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaObserver = new SchemaObserver();
        }
    }
}