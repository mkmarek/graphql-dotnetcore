namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ExecutionContext_IntroSpection
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_Introspecting__Schema_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__Schema_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_Introspecting__Type_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__Type_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Schema");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_Introspecting__TypeKind_HasKindENUM()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__TypeKind");
            Assert.AreEqual("ENUM", result.kind);
        }

        [Test]
        public void Execute_IntrospectingBoolean_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Boolean");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingBoolean_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Boolean");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingFloat_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Float");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingFloat_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Float");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingInt_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Int");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingInt_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "Int");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingRootQueryType_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "RootQueryType");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingRootQueryType_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "RootQueryType");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueENUM()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "ENUM"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueINPUT_OBJECT()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "INPUT_OBJECT"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueINTERFACE()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "INTERFACE"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueLIST()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "LIST"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueNON_NULL()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "NON_NULL"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueOBJECT()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "OBJECT"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueSCALAR()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "SCALAR"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasEnumValueUNION()
        {
            var result = (IEnumerable<dynamic>)GetType("__TypeKind").enumValues;
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "NON_NULL"));
        }

        [Test]
        public void Execute_IntrospectingSingleType__TypeKind_HasKindENUM()
        {
            var result = GetType("__TypeKind");
            Assert.AreEqual("ENUM", result.kind);
        }

        [Test]
        public void Execute_IntrospectingSingleTypeSumArgument_HasName()
        {
            var result = (IEnumerable<dynamic>)GetField("sum").args;
            Assert.AreEqual("numbers", result.SingleOrDefault().name);
        }

        [Test]
        public void Execute_IntrospectingSingleTypeSumArgument_HasOfTypeInt()
        {
            var result = (IEnumerable<dynamic>)GetField("sum").args;
            Assert.AreEqual("Int", result.SingleOrDefault().type.ofType.name);
        }

        [Test]
        public void Execute_IntrospectingSingleTypeSumArgument_HasTypeKindList()
        {
            var result = (IEnumerable<dynamic>)GetField("sum").args;
            Assert.AreEqual("LIST", result.SingleOrDefault().type.kind);
        }

        [Test]
        public void Execute_IntrospectingSingleTypeWithArguments_HasOneArgument()
        {
            var result = GetField("sum");
            Assert.AreEqual(1, result.args.Count);
        }

        [Test]
        public void Execute_IntrospectingString_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "String");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingString_IsTypeKindScalar()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "String");
            Assert.AreEqual("SCALAR", result.kind);
        }

        [Test]
        public void Execute_IntrospectingT1_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T1");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingT1_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T1");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_IntrospectingT2_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T2");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingT2_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "T2");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullDescription()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.description);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullIDeprecationReason()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.deprecationReason);
        }

        [Test]
        public void Execute_ObjectT1WithThreeFields_FieldAHasNullIsDeprecated()
        {
            var field = GetFieldForObject("T1", "a");
            Assert.IsNull(field.isDeprecated);
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
            type2.Field("sum", (int[] numbers) => numbers.Sum());

            rootType.Field("type1", () => type1);
            type1.Field("type1", () => type2);

            this.schema.SetRoot(rootType);
        }

        private dynamic GetField(string fieldName)
        {
            return ((IEnumerable<dynamic>)GetType("T2").fields).SingleOrDefault(e => e.name == fieldName);
        }

        private dynamic GetFieldForObject(string objectName, string fieldName)
        {
            return ((IEnumerable<dynamic>)GetSchemaTypes().SingleOrDefault(e => e.name == objectName).fields)
                .SingleOrDefault(e => e.name == fieldName);
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
                      kind
                    }
                  }
                }
              }
            }
            ").__schema;
        }

        private IEnumerable<dynamic> GetSchemaTypes()
        {
            return (IEnumerable<dynamic>)GetSchema().types;
        }

        private dynamic GetType(string typeName)
        {
            return this.schema.Execute(@"
            {
	          __type(name: " + "\"" + typeName + "\"" + @") {
                name,
                description,
                fields {
                  name
                  args {
                    name
                    type {
                      kind
                      ofType {
                        name
                      }
                    }
                  }
                },
                possibleTypes {
                  name
                },
                enumValues {
                  name,
                  description
                  isDeprecated,
                  deprecationReason
                },
                inputFields {
                  name
                },
                ofType {
                  name
                }
                kind
              }
            }
            ").__type;
        }

        private class TestType
        {
            public bool A { get; set; }
            public float B { get; set; }
            public int C { get; set; }
        }
    }
}