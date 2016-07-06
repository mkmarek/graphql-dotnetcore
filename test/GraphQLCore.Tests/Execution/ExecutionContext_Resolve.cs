namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_Resolve
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_AcessorBasedProperty_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ acessorBasedProp { Hello } }");

            Assert.AreEqual("world", result.acessorBasedProp.Hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_DoesntOutputFieldWhenNotSelected()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string a = result.test; }));
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.AreEqual("world", result.hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldWithAlias()
        {
            dynamic result = this.schema.Execute("{ aliased : hello, test }");

            Assert.AreEqual("world", result.aliased);
            Assert.AreEqual("test", result.test);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsTwoFieldsBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello, test }");

            Assert.AreEqual("world", result.hello);
            Assert.AreEqual("test", result.test);
        }

        [Test]
        public void Execute_FieldSelectionTwoIdenticalFieldsQuery_IgnoresDuplicates()
        {
            dynamic result = this.schema.Execute("{ hello, hello }");

            Assert.AreEqual("world", result.hello);
        }

        [Test]
        public void Execute_NestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { howdy } }");

            Assert.AreEqual("xzyt", result.nested.howdy);
        }

        [Test]
        public void Execute_NestedPropertyWithParentAlias_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ aliasedProp : acessorBasedProp { Test } }");

            Assert.AreEqual("stuff", result.aliasedProp.Test);
        }

        [Test]
        public void Execute_TwicelyNestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { anotherNested { stuff } } }");

            Assert.AreEqual("a", result.nested.anotherNested.stuff);
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

            rootType.Field("acessorBasedProp", () => new TestType() { Hello = "world", Test = "stuff" });

            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(anotherSestedType);
            this.schema.AddKnownType(nestedType);
            this.schema.AddKnownType(typeWithAccessor);
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

        public class NestedQueryType : GraphQLObjectType
        {
            public NestedQueryType() : base("NestedQueryType", "")
            {
            }
        }

        public class RootQueryType : GraphQLObjectType
        {
            public RootQueryType() : base("RootQueryType", "")
            {
                this.Field("hello", () => "world");
                this.Field("test", () => "test");
            }
        }

        public class TestType
        {
            public string Hello { get; set; }
            public string Test { get; set; }
        }
    }
}