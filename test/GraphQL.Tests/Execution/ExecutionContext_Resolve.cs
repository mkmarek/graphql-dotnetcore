namespace GraphQL.Tests.Execution
{
    using GraphQL.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;


    [TestFixture]
    public class ExecutionContext_Resolve
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.AreEqual("world", result.hello);
        }

        [Test]
        public void Execute_FieldSelectionQuery_DoesntOutputFieldWhenNotSelected()
        {
            dynamic result = this.schema.Execute("{ hello }");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string a = result.test; }));
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsTwoFieldsBasedOnResolver()
        {
            dynamic result = this.schema.Execute("{ hello, test }");

            Assert.AreEqual("world", result.hello);
            Assert.AreEqual("test", result.test);
        }

        [Test]
        public void Execute_FieldSelectionQuery_OutputsFieldWithAlias()
        {
            dynamic result = this.schema.Execute("{ aliased : hello, test }");

            Assert.AreEqual("world", result.aliased);
            Assert.AreEqual("test", result.test);
        }

        [Test]
        public void Execute_NestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { howdy } }");

            Assert.AreEqual("xzyt", result.nested.howdy);
        }

        [Test]
        public void Execute_TwicelyNestedFieldSelectionQuery_OutputsNestedField()
        {
            dynamic result = this.schema.Execute("{ nested { anotherNested { stuff } } }");

            Assert.AreEqual("a", result.nested.anotherNested.stuff);
        }

        [Test]
        public void Execute_AcessorBasedProperty_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ acessorBasedProp { Hello } }");

            Assert.AreEqual("world", result.acessorBasedProp.Hello);
        }

        [Test]
        public void Execute_NestedPropertyWithParentAlias_ReturnsDefinedValue()
        {
            dynamic result = this.schema.Execute("{ aliasedProp : acessorBasedProp { Test } }");

            Assert.AreEqual("stuff", result.aliasedProp.Test);
        }

        [SetUp]
        public void SetUp()
        {
            var rootType = new GraphQLObjectType("RootQueryType", "");
            rootType.Field("hello", () => "world");
            rootType.Field("test", () => "test");

            var nestedType = new GraphQLObjectType("NestedQueryType", "");
            nestedType.Field("howdy", () => "xzyt");

            var anotherSestedType = new GraphQLObjectType("AnotherNestedQueryType", "");
            anotherSestedType.Field("stuff", () => "a");

            nestedType.Field("anotherNested", () => anotherSestedType);
            rootType.Field("nested", () => nestedType);

            var typeWithAccessor = new GraphQLObjectType<TestType>("CustomObject", "test");
            typeWithAccessor.SetResolver(() => new TestType() { Hello = "world", Test = "stuff" });
            typeWithAccessor.AddField("Hello", e => e.Hello);
            typeWithAccessor.AddField("Test", e => e.Test);

            rootType.Field("acessorBasedProp", () => typeWithAccessor);
            this.schema = new GraphQLSchema(rootType);
        }

        public class TestType
        {
            public string Hello { get; set; }
            public string Test { get; set; }
        }
    }
}
