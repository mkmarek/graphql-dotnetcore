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
            var result = this.schema.Execute("{ acessorBasedProp { Hello } }");

            Assert.AreEqual("world", result.Data.acessorBasedProp.Hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_DoesntOutputFieldWhenNotSelected()
        {
            var result = this.schema.Execute("{ hello }");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string a = result.Data.test; }));
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldBasedOnResolver()
        {
            var result = this.schema.Execute("{ hello }");

            Assert.AreEqual("world", result.Data.hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldWithAlias()
        {
            var result = this.schema.Execute("{ aliased : hello, test }");

            Assert.AreEqual("world", result.Data.aliased);
            Assert.AreEqual("test", result.Data.test);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsTwoFieldsBasedOnResolver()
        {
            var result = this.schema.Execute("{ hello, test }");

            Assert.AreEqual("world", result.Data.hello);
            Assert.AreEqual("test", result.Data.test);
        }

        [Test]
        public void Execute_FieldSelectionTwoIdenticalFieldsQuery_IgnoresDuplicates()
        {
            var result = this.schema.Execute("{ hello, hello }");

            Assert.AreEqual("world", result.Data.hello);
        }

        [Test]
        public void Execute_NestedFieldSelectionQuery_OutputsNestedField()
        {
            var result = this.schema.Execute("{ nested { howdy } }");

            Assert.AreEqual("xzyt", result.Data.nested.howdy);
        }

        [Test]
        public void Execute_NestedPropertyWithParentAlias_ReturnsDefinedValue()
        {
            var result = this.schema.Execute("{ aliasedProp : acessorBasedProp { Test } }");

            Assert.AreEqual("stuff", result.Data.aliasedProp.Test);
        }

        [Test]
        public void Execute_TwicelyNestedFieldSelectionQuery_OutputsNestedField()
        {
            var result = this.schema.Execute("{ nested { anotherNested { stuff } } }");

            Assert.AreEqual("a", result.Data.nested.anotherNested.stuff);
        }

        [Test]
        public void Execute_ObjectField_ReturnsCorrectTypeName()
        {
            var result = this.schema.Execute(@"
            {
                acessorBasedProp {
                    __typename
                }
            }");

            Assert.AreEqual("CustomObject", result.Data.acessorBasedProp.__typename);
        }

        [Test]
        public void Execute_InterfaceField_ReturnsCorrectTypeName()
        {
            var result = this.schema.Execute(@"
            {
                testTypes {
                    __typename
                }
            }");

            Assert.AreEqual(2, result.Data.testTypes.Count);
            Assert.AreEqual("CustomObject", result.Data.testTypes[0].__typename);
            Assert.AreEqual("AnotherCustomObject", result.Data.testTypes[1].__typename);
        }

        [Test]
        public void Execute_Introspection_DoesNotReturnTypeNameField()
        {
            var result = this.schema.Execute(@"
            {
                __type(name: ""CustomObject"") {
                    fields {
                        name
                    }
                }
            }");

            var fieldNames = result.Data.__type.fields;

            Assert.IsFalse(fieldNames.Contains("__typename"));
        }

        [Test]
        public void Execute_Enum_CorrectlyReturnsEnumValue()
        {
            var result = this.schema.Execute(@"
            {
                enum
            }");

            var value = result.Data.@enum;

            Assert.AreEqual("One", value);
        }

        [Test]
        public void Execute_Introspection_ReturnsCorrectlyNonNullEnumField()
        {
            var result = this.schema.Execute(@"
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

            var fields = result.Data.__type.fields as IEnumerable<dynamic>;
            var emumField = fields.Single(e => e.name == "enum");

            Assert.AreEqual("NON_NULL", emumField.type.kind);
            Assert.AreEqual("TestEnum", emumField.type.ofType.name);
        }

        [Test]
        public void Execute_Introspection_ReturnsCorrectlyNullableEnumField()
        {
            var result = this.schema.Execute(@"
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

            var fields = result.Data.__type.fields as IEnumerable<dynamic>;
            var emumField = fields.Single(e => e.name == "nullableEnum");

            Assert.AreEqual("TestEnum", emumField.type.name);
        }

        [Test]
        public void Execute_NullableEnum_CorrectlyReturnsEnumValue()
        {
            var result = this.schema.Execute(@"
            {
                nullableEnum
            }");

            var value = result.Data.nullableEnum;

            Assert.AreEqual("Two", value);
        }

        [Test]
        public void Execute_NullableEnumWithNull_CorrectlyReturnsEnumValue()
        {
            var result = this.schema.Execute(@"
            {
                nullableEnumWithNull
            }");

            var value = result.Data.nullableEnumWithNull;

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

            var errors = result.Errors;

            Assert.IsNull(result.Data);
            Assert.AreEqual(3, errors.Count());
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
