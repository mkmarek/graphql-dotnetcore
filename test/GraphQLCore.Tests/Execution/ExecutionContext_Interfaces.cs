namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_Interfaces
    {
        private GraphQLSchema schema;

        public interface ITestObject
        {
            string Name { get; set; }
        }

        [Test]
        public void Execute_ScalarBasedTruthyIncludeDirective_PrintsTheField()
        {
            dynamic result = this.schema.Execute("{ nested { name } }");

            Assert.AreEqual("xzy", result.nested.name);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();

            var rootType = new RootQueryType( this.schema);
            var nestedType = new TestObjectType(this.schema);

            this.schema.SetRoot(rootType);
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType(GraphQLSchema schema) : base("RootQueryType", "", schema)
            {
                this.Field("nested", () => new TestObject() { Name = "xzy" });
            }
        }

        private class TestObjectType : GraphQLInterfaceType<ITestObject>
        {
            public TestObjectType(GraphQLSchema schema) : base("NestedQueryType", "", schema)
            {
                this.Field("name", e => e.Name);
            }
        }

        public class TestObject : ITestObject
        {
            public string Name { get; set; }
        }
    }
}