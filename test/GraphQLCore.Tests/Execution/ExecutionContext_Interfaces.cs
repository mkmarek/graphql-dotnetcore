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

            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);
            var nestedType = new GraphQLInterfaceType<ITestObject>("NestedQueryType", "", this.schema);
            nestedType.Field("name", e => e.Name);

            rootType.Field("nested", () => nestedType.WithValue(new TestObject() { Name = "xzy" }));

            this.schema.SetRoot(rootType);
        }

        public class TestObject : ITestObject
        {
            public string Name { get; set; }
        }
    }
}