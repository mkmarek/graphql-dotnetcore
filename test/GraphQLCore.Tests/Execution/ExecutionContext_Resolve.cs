namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    [TestFixture]
    public class ExecutionContext_Resolve
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_AcessorBasedProperty_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ acessorBasedProp { Hello } }");

            Assert.AreEqual("world", result.data.acessorBasedProp.Hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_DoesntOutputFieldWhenNotSelected()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string a = result.data.test; }));
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.AreEqual("world", result.data.hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldWithAlias()
        {
            dynamic result = this.schema.Execute("{ aliased : hello, test }");

            Assert.AreEqual("world", result.data.aliased);
            Assert.AreEqual("test", result.data.test);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsTwoFieldsBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello, test }");

            Assert.AreEqual("world", result.data.hello);
            Assert.AreEqual("test", result.data.test);
        }

        [Test]
        public void Execute_FieldSelectionTwoIdenticalFieldsQuery_IgnoresDuplicates()
        {
            dynamic result = this.schema.Execute("{ hello, hello }");

            Assert.AreEqual("world", result.data.hello);
        }

        [Test]
        public void Execute_NestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { howdy } }");

            Assert.AreEqual("xzyt", result.data.nested.howdy);
        }

        [Test]
        public void Execute_NestedPropertyWithParentAlias_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ aliasedProp : acessorBasedProp { Test } }");

            Assert.AreEqual("stuff", result.data.aliasedProp.Test);
        }

        [Test]
        public void Execute_TwicelyNestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { anotherNested { stuff } } }");

            Assert.AreEqual("a", result.data.nested.anotherNested.stuff);
        }

        [Test]
        public void Execute_ObjectField_ReturnsCorrectTypeName()
        {
            dynamic result = this.schema.Execute(@"
            {
                acessorBasedProp {
                    __typename
                }
            }");

            Assert.AreEqual("CustomObject", result.data.acessorBasedProp.__typename);
        }

        [Test]
        public void Execute_InterfaceField_ReturnsCorrectTypeName()
        {
            dynamic result = this.schema.Execute(@"
            {
                testTypes {
                    __typename
                }
            }");

            Assert.AreEqual(2, result.data.testTypes.Count);
            Assert.AreEqual("CustomObject", result.data.testTypes[0].__typename);
            Assert.AreEqual("AnotherCustomObject", result.data.testTypes[1].__typename);
        }

        [Test]
        public void Execute_Introspection_DoesNotReturnTypeNameField()
        {
            dynamic result = this.schema.Execute(@"
            {
                __type(name: ""CustomObject"") {
                    fields {
                        name
                    }
                }
            }");

            var fieldNames = result.data.__type.fields;

            Assert.IsFalse(fieldNames.Contains("__typename"));
        }

        [Test]
        public void Execute_Enum_CorrectlyReturnsEnumValue()
        {
            dynamic result = this.schema.Execute(@"
            {
                enum
            }");

            var value = result.data.@enum;

            Assert.AreEqual("One", value);
        }

        [Test]
        public void Execute_Introspection_ReturnsCorrectlyNonNullEnumField()
        {
            dynamic result = this.schema.Execute(@"
            {
                __type(name: ""RootQueryType"") {
                    fields {
                        name
                        type {
                            ofType {
                                name
                            }
                            kind
                        }
                    }
                }
            }");

            var fields = result.data.__type.fields as IEnumerable<dynamic>;
            var emumField = fields.Single(e => e.name == "enum");

            Assert.AreEqual("NON_NULL", emumField.type.kind);
            Assert.AreEqual("TestEnum", emumField.type.ofType.name);
        }

        [Test]
        public void Execute_Introspection_ReturnsCorrectlyNullableEnumField()
        {
            dynamic result = this.schema.Execute(@"
            {
                __type(name: ""RootQueryType"") {
                    fields {
                        name
                        type {
                            name
                        }
                    }
                }
            }");

            var fields = result.data.__type.fields as IEnumerable<dynamic>;
            var emumField = fields.Single(e => e.name == "nullableEnum");

            Assert.AreEqual("TestEnum", emumField.type.name);
        }

        [Test]
        public void Execute_NullableEnum_CorrectlyReturnsEnumValue()
        {
            dynamic result = this.schema.Execute(@"
            {
                nullableEnum
            }");

            var value = result.data.nullableEnum;

            Assert.AreEqual("Two", value);
        }

        [Test]
        public void Execute_NullableEnumWithNull_CorrectlyReturnsEnumValue()
        {
            dynamic result = this.schema.Execute(@"
            {
                nullableEnumWithNull
            }");

            var value = result.data.nullableEnumWithNull;

            Assert.AreEqual(null, value);
        }

        [Test]
        public void Execute_DuplicateOperations_ShouldNotReportAnyOtherExectionThenValidationException()
        {
            var result = this.schema.Execute(@"
            query fetch {
                nullableEnumWithNull
            }

            query fetch {
                nullableEnumWithNull
            }

            mutation fetch {
                nullableEnumWithNull
            }

            mutation fetch {
                nullableEnumWithNull
            }
            ", null, "fetch");

            var errors = result.errors as IList<GraphQLException>;
            var objectResult = result as IDictionary<string, object>;

            Assert.IsFalse(objectResult.ContainsKey("data"));
            Assert.AreEqual(3, errors.Count);
        }



        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();

            var rootType = new RootQueryType();
            var nestedType = new NestedQueryType();
            nestedType.Field("howdy", () => "xzyt");

            var anotherSestedType = new AnotherNestedQueryType();
            anotherSestedType.Field("stuff", () => "a");

            nestedType.Field("anotherNested", () => anotherSestedType);
            rootType.Field("nested", () => nestedType);

            var typeWithAccessor = new CustomObject();
            typeWithAccessor.Field("Hello", e => e.Hello);
            typeWithAccessor.Field("Test", e => e.Test);

            var anotherTypeWithAccessor = new AnotherCustomObject();
            anotherTypeWithAccessor.Field("Hello", e => e.Hello);
            anotherTypeWithAccessor.Field("World", e => e.World);

            var customInterface = new CustomInterface();
            customInterface.Field("hello", e => e.Hello);

            rootType.Field("acessorBasedProp", () => new TestType() { Hello = "world", Test = "stuff" });
            rootType.Field("testTypes", () => new ITestInterface[] {
                new TestType { Hello = "world", Test = "stuff" },
                new AnotherTestType { Hello = "world", World = "hello" }
            });

            this.schema.AddKnownType(new TestEnumType());
            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(anotherSestedType);
            this.schema.AddKnownType(nestedType);
            this.schema.AddKnownType(typeWithAccessor);
            this.schema.AddKnownType(anotherTypeWithAccessor);
            this.schema.AddKnownType(customInterface);
            this.schema.Query(rootType);
        }

        public class AnotherNestedQueryType : GraphQLObjectType
        {
            public AnotherNestedQueryType() : base("AnotherNestedQueryType", "")
            {
            }
        }

        public class CustomObject : GraphQLObjectType<TestType>
        {
            public CustomObject() : base("CustomObject", "")
            {
            }
        }

        public class AnotherCustomObject : GraphQLObjectType<AnotherTestType>
        {
            public AnotherCustomObject() : base("AnotherCustomObject", "")
            {
            }
        }

        public class CustomInterface : GraphQLInterfaceType<ITestInterface>
        {
            public CustomInterface() : base("CustomInterface", "")
            {
            }
        }

        public class NestedQueryType : GraphQLObjectType
        {
            public NestedQueryType() : base("NestedQueryType", "")
            {
            }
        }

        public enum TestEnum { One, Two }

        public class TestEnumType : GraphQLEnumType<TestEnum>
        {
            public TestEnumType() : base("TestEnum", "")
            {
            }
        }

        public class RootQueryType : GraphQLObjectType
        {
            public RootQueryType() : base("RootQueryType", "")
            {
                this.Field("hello", () => "world");
                this.Field("test", () => "test");
                this.Field("enum", () => TestEnum.One);
                this.Field("nullableEnum", () => TestEnum.Two as TestEnum?);
                this.Field("nullableEnumWithNull", () => null as TestEnum?);
            }
        }

        public class TestType : ITestInterface
        {
            public string Hello { get; set; }
            public string Test { get; set; }
        }

        public class AnotherTestType : ITestInterface
        {
            public string Hello { get; set; }
            public string World { get; set; }
        }

        public interface ITestInterface
        {
            string Hello { get; set; }
        } 
    }
}
