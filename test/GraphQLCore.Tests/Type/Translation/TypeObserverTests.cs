namespace GraphQLCore.Tests.Type.Translation
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalars;
    using GraphQLCore.Type.Translation;
    using NSubstitute;
    using NUnit.Framework;
    using Schemas;
    using System.Linq;

    [TestFixture]
    public class TypeObserverTests
    {
        private IObjectTypeTranslator complicatedArgsObserver;
        private IObjectTypeTranslator complicatedObjectTypeObserver;
        private ISchemaObserver schemaObserver;
        private ITypeTranslator typeTranslator;

        [Test]
        public void GetField_FieldWithSingleArg_GetsArgumentTypeFromTypeTranslator()
        {
            var field = complicatedArgsObserver.GetField("intArgField");

            Assert.IsInstanceOf<GraphQLInt>(field.Arguments.Single().Value);
            Assert.AreEqual("intArg", field.Arguments.Single().Key);
        }

        [Test]
        public void GetField_ObjectType_GetsTypeFromTypeTranslator()
        {
            var field = complicatedObjectTypeObserver.GetField("intField");

            Assert.IsInstanceOf<GraphQLInt>(field.Type);
        }

        [Test]
        public void GetField_ObjectType_ReturnsCorrectName()
        {
            var field = complicatedObjectTypeObserver.GetField("intField");

            Assert.AreEqual("intField", field.Name);
            Assert.IsNull(field.Description);
        }

        [Test]
        public void GetField_ObjectType_ReturnsCorrectNameAndDescription()
        {
            var field = complicatedObjectTypeObserver.GetField("intField");

            Assert.AreEqual("intField", field.Name);
            Assert.IsNull(field.Description);
        }

        [Test]
        public void GetFields_ObjectType_ReturnsTranslatedFields()
        {
            var fields = complicatedObjectTypeObserver.GetFields();

            Assert.AreEqual(7, fields.Count());
        }

        [SetUp]
        public void SetUp()
        {
            this.typeTranslator = Substitute.For<ITypeTranslator>();
            this.schemaObserver = Substitute.For<ISchemaObserver>();
            this.typeTranslator.GetType(typeof(int?)).Returns(new GraphQLInt());

            this.complicatedObjectTypeObserver = new ObjectTypeTranslator(new ComplicatedObjectType(), this.typeTranslator, this.schemaObserver);
            this.complicatedArgsObserver = new ObjectTypeTranslator(new ComplicatedArgs(), this.typeTranslator, this.schemaObserver);
        }
    }
}