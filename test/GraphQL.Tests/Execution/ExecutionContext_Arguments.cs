namespace GraphQL.Tests.Execution
{
    using GraphQL.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_Arguments
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_EntityFetchedWithArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { Id, StringField } }");

            Assert.AreEqual(42, result.nested.Id);
            Assert.AreEqual("Test with id 42", result.nested.StringField);
        }

        [Test]
        public void Execute_NestedEntityFetchedWithArguments_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { nested { text(str: \"string argument\") } } }");

            Assert.AreEqual("42 is from the parent and string argument is the current type", result.nested.nested.text);
        }

        [SetUp]
        public void SetUp()
        {
            var rootType = new GraphQLObjectType("RootQueryType", "");

            var nestedType = new GraphQLObjectType<TestObject>("NestedQueryType", "");
            nestedType.SetResolver((int id) => new TestObject() { Id = id, StringField = "Test with id " + id });
            nestedType.Field(instance => instance.Id);
            nestedType.Field(instance => instance.StringField);

            var nestedTypeNonGeneric = new GraphQLObjectType("NestedNonGenericQueryType", "");
            nestedTypeNonGeneric.Field("text", (int id, string str) => $"{id} is from the parent and {str} is the current type");

            nestedType.Field("nested", () => nestedTypeNonGeneric);
            rootType.Field("nested", () => nestedType);

            this.schema = new GraphQLSchema(rootType);
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string StringField { get; set; }
        }
    }
}
