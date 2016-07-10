namespace GraphQLCore.Tests.Type.Introspection
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Introspection;
    using GraphQLCore.Type.Scalars;
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using Schemas;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class IntrospectorTests
    {
        private Introspector introspector;

        [Test]
        public void Introspect_EnumType_ReturnsTypeWithCorrectProperties()
        {
            var introspectedTypeObject = this.introspector.Introspect(new FurColorEnum());

            Assert.AreEqual("FurColor", introspectedTypeObject.Name);
            Assert.AreEqual(TypeKind.ENUM, introspectedTypeObject.Kind);
            Assert.IsNull(introspectedTypeObject.OfType);
            Assert.IsNotNull(introspectedTypeObject.Description);

            Assert.AreEqual(4, introspectedTypeObject.EnumValues.Count());
        }

        [Test]
        public void Introspect_NonNullScalarType_ReturnsTypeWithCorrectProperties()
        {
            var introspectedTypeObject = this.introspector.Introspect(new GraphQLNonNullType(new GraphQLInt()));

            Assert.AreEqual(TypeKind.NON_NULL, introspectedTypeObject.Kind);
            Assert.AreEqual("Int", introspectedTypeObject.OfType.Name);
            Assert.AreEqual(TypeKind.SCALAR, introspectedTypeObject.OfType.Kind);
            Assert.IsNotNull(introspectedTypeObject.OfType.Description);
        }

        [Test]
        public void Introspect_ObjectType_ReturnsSingleArgumentWithCorrectProps()
        {
            var introspectedTypeObject = this.introspector.Introspect(new ComplicatedArgs());

            var intArgField = introspectedTypeObject.Fields.Single(e => e.Name == "intArgField");

            Assert.AreEqual("intArg", intArgField.Arguments.Single().Name);
        }

        [Test]
        public void Introspect_ObjectType_ReturnsSingleFieldtWithCorrectProps()
        {
            var introspectedTypeObject = this.introspector.Introspect(new QueryRoot());

            Assert.AreEqual("complicatedArgs", introspectedTypeObject.Fields.Single().Name);
            Assert.AreEqual("ComplicatedArgs", introspectedTypeObject.Fields.Single().Type.Name);
        }

        [Test]
        public void Introspect_ScalarType_ReturnsTypeWithCorrectProperties()
        {
            var introspectedTypeObject = this.introspector.Introspect(new GraphQLInt());

            Assert.AreEqual("Int", introspectedTypeObject.Name);
            Assert.AreEqual(TypeKind.SCALAR, introspectedTypeObject.Kind);
            Assert.IsNull(introspectedTypeObject.OfType);
            Assert.IsNotNull(introspectedTypeObject.Description);
        }

        [Test]
        public void IntrospectField_IntField_ReturnsCorrectFieldProperties()
        {
            var field = this.introspector.IntrospectField(new GraphQLFieldConfig()
            {
                Name = "IntField",
                Arguments = new Dictionary<string, GraphQLScalarType>(),
                Type = new GraphQLInt(),
                Description = "SomeDescription"
            });

            Assert.AreEqual("IntField", field.Name);
            Assert.AreEqual("Int", field.Type.Name);
            Assert.AreEqual("SomeDescription", field.Description);
        }

        [Test]
        public void IntrospectField_InputObjectType_ReturnsCorrectProperties()
        {
            var inputObject = this.introspector.Introspect(new ComplicatedInputObjectType());

            Assert.AreEqual(TypeKind.INPUT_OBJECT, inputObject.Kind);
            Assert.AreEqual("ComplicatedInputObjectType", inputObject.Name);
            Assert.AreEqual("ComplicatedInputObjectType description", inputObject.Description);
        }

        [Test]
        public void IntrospectField_InputObjectType_ReturnsCorrectNumberOfInputFieldsProperties()
        {
            var inputObject = this.introspector.Introspect(new ComplicatedInputObjectType());

            Assert.AreEqual(8, inputObject.InputFields.Count());
        }

        [Test]
        public void IntrospectField_InputObjectType_ReturnsNestedInputTypeWithCorrectProperties()
        {
            var inputObject = this.introspector.Introspect(new ComplicatedInputObjectType());

            var nestedField = inputObject.InputFields.Where(e => e.Name == "nested").Single();

            Assert.AreEqual(TypeKind.INPUT_OBJECT, nestedField.Type.Kind);
            Assert.AreEqual("ComplicatedInputObjectType", nestedField.Type.Name);
            Assert.AreEqual("ComplicatedInputObjectType description", nestedField.Type.Description);
        }

        [SetUp]
        public void SetUp()
        {
            var schemaObserver = new SchemaObserver();
            schemaObserver.AddKnownType(new QueryRoot());
            schemaObserver.AddKnownType(new ComplicatedArgs());
            schemaObserver.AddKnownType(new FurColorEnum());
            schemaObserver.AddKnownType(new ComplicatedObjectType());
            schemaObserver.AddKnownType(new ComplicatedInputObjectType());

            var typeTranslator = new TypeTranslator(schemaObserver);
            this.introspector = new Introspector(typeTranslator);
        }
    }
}