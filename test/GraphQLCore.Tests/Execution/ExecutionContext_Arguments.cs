namespace GraphQLCore.Tests.Execution
{
    using Exceptions;
    using GraphQLCore.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    [TestFixture]
    public class ExecutionContext_Arguments
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_EntityFetchedWithIntegerArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { Id, StringField } }");

            Assert.AreEqual(42, result.nested.Id);
            Assert.AreEqual("Test with id 42", result.nested.StringField);
        }

        [Test]
        public void Execute_NestedEntityFetchedWithArguments_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { nested(id: 24) { text(str: \"string argument\") } } }");

            Assert.AreEqual("24 is from the parent and string argument is the current type", result.nested.nested.text);
        }

        [Test]
        public void Execute_ArrayViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withArray(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withArray);
        }

        [Test]
        public void Execute_ListViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withList(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withList);
        }

        [Test]
        public void Execute_IEnumerableViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withIEnumerable(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withIEnumerable);
        }

        [Test]
        public void Execute_EntityFetchedWithStringArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: \"42\") { Id, StringField } }");

            Assert.AreEqual(42, result.nested.Id);
            Assert.AreEqual("Test with id 42", result.nested.StringField);
        }

        [Test]
        public void Execute_EntityFetchedWithBooleanTrueValuedArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: true) { Id, StringField } }");

            Assert.AreEqual(1, result.nested.Id);
            Assert.AreEqual("Test with id 1", result.nested.StringField);
        }

        [Test]
        public void Execute_EntityFetchedWithBooleanFalseValuedArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: false) { Id, StringField } }");

            Assert.AreEqual(0, result.nested.Id);
            Assert.AreEqual("Test with id 0", result.nested.StringField);
        }

        [Test]
        public void Execute_EntityFetchedWithFloatArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42.5) { Id, StringField } }");

            Assert.AreEqual(42, result.nested.Id);
            Assert.AreEqual("Test with id 42", result.nested.StringField);
        }

        [Test]
        public void Execute_EntityFetchedWithListIdArgument_ThrowsException()
        {
            var exception = Assert.Throws<GraphQLException>(new TestDelegate(() => this.schema.Execute("{ nested(id: [1,2,3]) { Id, StringField } }")));

            Assert.AreEqual("Can't convert input of type List`1 to Int32.", exception.Message);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);

            var nestedType = new GraphQLObjectType<TestObject>("NestedQueryType", "", this.schema);
            nestedType.SetResolver((int id) => new TestObject() { Id = id, StringField = "Test with id " + id });
            nestedType.Field(instance => instance.Id);
            nestedType.Field(instance => instance.StringField);

            var nestedTypeNonGeneric = new GraphQLObjectType("NestedNonGenericQueryType", "", this.schema);
            nestedTypeNonGeneric.Field("text", (int id, string str) => $"{id} is from the parent and {str} is the current type");

            nestedType.Field("nested", () => nestedTypeNonGeneric);
            rootType.Field("nested", () => nestedType);

            rootType.Field("withArray", (int[] ids) => ids.Count());
            rootType.Field("withList", (List<int> ids) => ids.Count());
            rootType.Field("withIEnumerable", (IEnumerable<int> ids) => ids.Count());

            this.schema.SetRoot(rootType);
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string StringField { get; set; }
        }
    }
}
