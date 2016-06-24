namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Introspection;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ExecutionContext_IntroSpection
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_IntrospectingRootQueryType_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "RootQueryType");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_IntrospectingT1_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T1");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_IntrospectingT2_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T2");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_Introspecting__Schema_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_Introspecting__Type_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_IntrospectingInt_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Int");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingBoolean_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Boolean");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingString_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "String");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingFloat_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Float");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingRootQueryType_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "RootQueryType");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingT1_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T1");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingT2_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T2");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__Schema_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__Type_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingInt_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Int");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingBoolean_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Boolean");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingString_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "String");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingFloat_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Float");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullDescription()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.description);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullIsDeprecated()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.isDeprecated);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullIDeprecationReason()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.deprecationReason);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasStringType()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.AreEqual("String", field.type.name);
        }

        [Test]
        public void Execute_QueryType_GetsTheRootObjectName()
        {
            var schema = GetSchema();
            Assert.AreEqual("RootQueryType", schema.queryType.name);
        }

        private dynamic GetFieldForObject(string objectName, string fieldName)
        {
            return ((IEnumerable<dynamic>)GetSchemaTypes().SingleOrDefault(e => e.name == objectName).fields)
                .SingleOrDefault(e => e.name == fieldName);
        }

        private IEnumerable<dynamic> GetSchemaTypes()
        {
            return (IEnumerable<dynamic>)GetSchema().types;
        }

        private dynamic GetSchema()
        {
            return this.schema.Execute(@"
            {
              __schema {
                queryType {
                  name
                }
                types {
                  name
                  kind
                  description
                  fields {
                    name
                    description
                    isDeprecated
                    deprecationReason
                    type {
                      name
                    }
                  }
                }
              }
            }
            ").__schema;
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);

            var type1 = new GraphQLObjectType("T1", "", this.schema);
            type1.Field("a", () => "1");
            type1.Field("b", () => 2);
            type1.Field("c", () => new int[] { 1, 2, 3 });

            var type2 = new GraphQLObjectType<TestType>("T2", "", this.schema);
            type2.Field("a", e => e.A);
            type2.Field("b", e => e.B);

            rootType.Field("type1", () => type1);
            type1.Field("type1", () => type2);

            this.schema.SetRoot(rootType);
        }

        private class TestType
        {
            public bool A { get; set; }
            public float B { get; set; }
            public int C { get; set; }
        }
    }
}
