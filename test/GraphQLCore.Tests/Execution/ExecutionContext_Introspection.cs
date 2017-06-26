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

        private interface ITestType
        {
            bool A { get; set; }
            float B { get; set; }
        }

        [Test]
        public void Execute_Introspecting__InputValue_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__InputValue");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__InputValue_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__InputValue");
            Assert.AreEqual("OBJECT", result.kind);
        }

        [Test]
        public void Execute_Introspecting__Type_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Type");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_Introspecting__Type_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Type");
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
        public void Execute_IntrospectingIntrospectedSchemaType_HasDescription()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Type");
            Assert.IsNotNull(result.description);
        }

        [Test]
        public void Execute_IntrospectingIntrospectedSchemaType_IsTypeKindObject()
        {
            var result = GetSchemaTypes().SingleOrDefault(e => e.name == "__Type");
            Assert.AreEqual("OBJECT", result.kind);
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
        public void Execute_IntrospectingSingleTypeSumArgument_HasOfTypeNonNullListOfInt()
        {
            var result = (IEnumerable<dynamic>)GetField("sum").args;

            Assert.AreEqual("NON_NULL", result.SingleOrDefault().type.ofType.kind);
            Assert.AreEqual("Int", result.SingleOrDefault().type.ofType.ofType.name);
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
        public void Execute_ObjectT1WithThreeFields_FieldBHasDescription()
        {
            var field = GetFieldForObject("T1", "b");
            Assert.AreEqual("description for b", field.description);
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
            Assert.IsFalse(field.isDeprecated);
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

        [Test]
        public void Execute_RootQueryField_HasDescription()
        {
            var field = GetRootField("type1");
            Assert.AreEqual("test", field.description);
        }

        [Test]
        public void Execute_RootQueryType2IField_ShouldBeInterfaceType()
        {
            var field = GetFieldForObject("RootQueryType", "type2i");
            Assert.AreEqual("T2Interface", field.type.name);
        }

        [Test]
        public void Execute_T1Typetype2Field_ShouldBeT2Type()
        {
            var field = GetFieldForObject("T1", "type2");

            Assert.AreEqual("T2", field.type.name);
        }

        [Test]
        public void SampleInputObjectType_HasInputObjectKind()
        {
            dynamic inputObject = GetType("SampleInputObjectType");

            Assert.AreEqual("INPUT_OBJECT", inputObject.kind);
        }

        [Test]
        public void Directives_ContainsCorrectDataForDirectives()
        {
            IEnumerable<dynamic> directives = GetSchema().directives;

            var includeDirective = directives.Single(e => e.name == "include");
            var skipDirective = directives.Single(e => e.name == "skip");

            Assert.AreEqual(new dynamic[] { "FIELD", "FRAGMENT_SPREAD", "INLINE_FRAGMENT" }, 
                includeDirective.locations);

            Assert.AreEqual(new dynamic[] { "FIELD", "FRAGMENT_SPREAD", "INLINE_FRAGMENT" }, 
                skipDirective.locations);

            Assert.AreEqual("if", includeDirective.args[0].name);
            Assert.AreEqual("NON_NULL", includeDirective.args[0].type.kind);
            Assert.AreEqual("Boolean", includeDirective.args[0].type.ofType.name);

            Assert.AreEqual("if", skipDirective.args[0].name);
            Assert.AreEqual("NON_NULL", skipDirective.args[0].type.kind);
            Assert.AreEqual("Boolean", skipDirective.args[0].type.ofType.name);
        }

        [Test]
        public void T2_HasOneInterface()
        {
            dynamic type = GetType("T2");

            Assert.AreEqual(1, type.interfaces.Count);
        }

        [Test]
        public void T2_HasOneInterfaceWithInterfaceKind()
        {
            dynamic type = GetType("T2");

            Assert.AreEqual("INTERFACE", ((IEnumerable<dynamic>)type.interfaces).First().kind);
        }

        [Test]
        public void T2Inteface_HasOnePossibleType()
        {
            dynamic type = GetType("T2Interface");

            Assert.AreEqual(1, ((IEnumerable<dynamic>)type.possibleTypes).Count());
        }

        [Test]
        public void T2Inteface_SinglePossibleTypeIsT2Object()
        {
            dynamic type = GetType("T2Interface");

            Assert.AreEqual("T2", ((IEnumerable<dynamic>)type.possibleTypes).SingleOrDefault().name);
        }

        [Test]
        public void NoMutationDefined_ShouldReturnNullInIntrospection()
        {
            var emptySchema = new GraphQLSchema();
            emptySchema.Query(new T1());

            var result = emptySchema.Execute("{ __schema { mutationType { name } } }");

            Assert.IsNull(result.data.__schema.mutationType);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();

            var type2 = new T2(this.schema);
            var type1 = new T1();
            var t2interface = new T2Interface(this.schema);
            var rootType = new RootQueryType(type1);

            this.schema.AddKnownType(type1);
            this.schema.AddKnownType(type2);
            this.schema.AddKnownType(t2interface);
            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(new SampleInputObjectType());

            this.schema.Query(rootType);
        }

        private dynamic GetRootField(string fieldName)
        {
            return ((IEnumerable<dynamic>)GetType("RootQueryType").fields).SingleOrDefault(e => e.name == fieldName);
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
              query IntrospectionQuery {
                __schema {
                  queryType { name }
                  mutationType { name }
                  subscriptionType { name }
                  types {
                    ...FullType
                  }
                  directives {
                    name
                    description
                    locations
                    args {
                      ...InputValue
                    }
                  }
                }
              }

              fragment FullType on __Type {
                kind
                name
                description
                fields(includeDeprecated: true) {
                  name
                  description
                  args {
                    ...InputValue
                  }
                  type {
                    ...TypeRef
                  }
                  isDeprecated
                  deprecationReason
                }
                inputFields {
                  ...InputValue
                }
                interfaces {
                  ...TypeRef
                }
                enumValues(includeDeprecated: true) {
                  name
                  description
                  isDeprecated
                  deprecationReason
                }
                possibleTypes {
                  ...TypeRef
                }
              }

              fragment InputValue on __InputValue {
                name
                description
                type { ...TypeRef }
                defaultValue
              }

              fragment TypeRef on __Type {
                kind
                name
                ofType {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                        ofType {
                          kind
                          name
                          ofType {
                            kind
                            name
                            ofType {
                              kind
                              name
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            ").data.__schema;
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
                possibleTypes {
                      name
                      kind
                      description
                      fields {
                        name
                        description
                      }
                      interfaces {
                        name
                      }
                    }
                interfaces {
                    name kind description
                },
                fields {
                  name
                  description
                  type {
                    name
                    kind
                    ofType {
                        name
                        kind
                        ofType {
                            name
                            kind
                        }
                    }
                  }
                  args {
                    name
                    type {
                      kind
                      ofType {
                        name
                        kind
                        ofType {
                          name
                          kind
                        }
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
            ").data.__type;
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType(T1 type1) : base("RootQueryType", "")
            {
                this.Field("type1", () => type1).WithDescription("test");
                this.Field("type2i", () => (ITestType)new TestType());
            }
        }

        private class SampleInputObjectType : GraphQLInputObjectType<TestType>
        {
            public SampleInputObjectType() : base("SampleInputObjectType", "")
            {
                this.Field("a", e => e.A);
                this.Field("b", e => e.B);
            }
        }

        private class T1 : GraphQLObjectType
        {
            public T1() : base("T1", "")
            {
                this.Field("a", () => "1");
                this.Field("b", () => 2).WithDescription("description for b");
                this.Field("c", () => new int[] { 1, 2, 3 });
                this.Field("type2", () => new TestType());
            }
        }

        private class T2 : GraphQLObjectType<TestType>
        {
            public T2(GraphQLSchema schema) : base("T2", "")
            {
                this.Field("a", e => e.A);
                this.Field("b", e => e.B);
                this.Field("sum", (int[] numbers) => numbers.Sum());
            }
        }

        private class T2Interface : GraphQLInterfaceType<ITestType>
        {
            public T2Interface(GraphQLSchema schema) : base("T2Interface", "")
            {
                this.Field("a", e => e.A);
                this.Field("b", e => e.B);
            }
        }

        private class TestType : ITestType
        {
            public bool A { get; set; }
            public float B { get; set; }
            public int C { get; set; }
        }
    }
}