namespace GraphQL.Tests.Type
{
    using GraphQL.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class GraphQLSchemaTests
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_MultipleOperationsNoOperationName_ThrowsAnError()
        {
            var exception = Assert.Throws<Exception>(new TestDelegate(() =>
                this.schema.Execute("query Example { hello } query OtherExample { hello }")));

            Assert.AreEqual("Must provide operation name if query contains multiple operations.", exception.Message);
        }

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

        [SetUp]
        public void SetUp()
        {
            var rootType = new GraphQLObjectType<TestRootType>("RootQueryType", "");
            rootType.AddField("hello", () => "world");
            rootType.AddField("test",  () => "test");

            this.schema = new GraphQLSchema(rootType);
        }

        public class TestRootType
        {
            public string Hello { get; set; }
            public string Test { get; set; }
        }
    }
}
