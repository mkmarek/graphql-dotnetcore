namespace GraphQLCore.Tests.Execution
{
    using NUnit.Framework;
    using Schemas;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ExecutionContext_InputObjectTypes
    {
        private TestSchema schema;

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasAnIntInputField()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("intField", GetFieldByName(complexInputType, "intField").name);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasAnIntInputFieldWithKindScalar()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("SCALAR", GetFieldByName(complexInputType, "intField").type.kind);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasAnIntInputFieldWithTypeNameInt()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("Int", GetFieldByName(complexInputType, "intField").type.name);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasNonNullIntInputField()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("nonNullIntField", GetFieldByName(complexInputType, "nonNullIntField").name);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasNonNullIntInputFieldWithKindNonNull()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("NON_NULL", GetFieldByName(complexInputType, "nonNullIntField").type.kind);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasNonNullIntInputFieldWithUnderlyingKindScalar()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("SCALAR", GetFieldByName(complexInputType, "nonNullIntField").type.ofType.kind);
        }

        [Test]
        public void Execute_IntrospectInputType_InputObjectHasNonNullIntInputFieldWithUnderlyingTypeNameInt()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("Int", GetFieldByName(complexInputType, "nonNullIntField").type.ofType.name);
        }

        [Test]
        public void Execute_IntrospectInputType_ReturnsObjectOfInputTypeKind()
        {
            var complexInputType = this.GetType("ComplicatedInputObjectType");

            Assert.AreEqual("INPUT_OBJECT", complexInputType.kind);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new TestSchema();
        }

        private static dynamic GetFieldByName(dynamic complexInputType, string fieldName)
        {
            return ((IEnumerable<dynamic>)complexInputType.inputFields).Where(e => e.name == fieldName).Single();
        }

        private dynamic GetType(string typeName)
        {
            return this.schema.Execute(@"
            {
	          __type(name: " + "\"" + typeName + "\"" + @") {
                name,
                description,
                inputFields {
                  name
                  type { kind name ofType { kind name } }
                },
                kind
              }
            }
            ").data.__type;
        }
    }
}