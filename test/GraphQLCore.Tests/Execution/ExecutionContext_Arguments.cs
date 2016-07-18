namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class ExecutionContext_Arguments
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_ArrayViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withArray(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withArray);
        }

        [Test]
        public void Execute_EntityFetchedWithIntegerArgument_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { Id, StringField } }");

            Assert.AreEqual(42, result.nested.Id);
            Assert.AreEqual("Test with id 42", result.nested.StringField);
        }

        [Test]
        public void Execute_FloatViaArgument_PrintsCorrectFloat()
        {
            dynamic result = this.schema.Execute("{ withFloat(value: 3.14) }");

            Assert.AreEqual(3.14f, result.withFloat);
        }

        [Test]
        public void Execute_FloatViaArgumentWithInt_PrintsCorrectFloat()
        {
            dynamic result = this.schema.Execute("{ withFloat(value: 3) }");

            Assert.AreEqual(3.0f, result.withFloat);
        }

        [Test]
        public void Execute_IEnumerableViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withIEnumerable(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withIEnumerable);
        }

        [Test]
        public void Execute_ListViaArgument_PrintsCorrectList()
        {
            dynamic result = this.schema.Execute("{ withList(ids: [1,2,3]) }");

            Assert.AreEqual(3, result.withList);
        }

        [Test]
        public void Execute_NestedEntityFetchedWithArguments_PrintsCorrectValues()
        {
            dynamic result = this.schema.Execute("{ nested(id: 42) { nested(id: 24) { text(str: \"string argument\") } } }");

            Assert.AreEqual("24 is from the parent and string argument is the current type", result.nested.nested.text);
        }

        [Test]
        public void Execute_NothingInNonMandatoryValue_InvokesResolverWithNullValue()
        {
            dynamic result = this.schema.Execute("{ isNull }");

            Assert.AreEqual(true, result.isNull);
        }

        [Test]
        public void Execute_ValueInNonMandatoryValue_InvokesResolverWithNullValue()
        {
            dynamic result = this.schema.Execute("{ isNull(nonMandatory: 1) }");

            Assert.AreEqual(false, result.isNull);
        }

        [Test]
        public void Execute_AstObjectArgument_InvokesResolverWithNullValue()
        {
            dynamic result = this.schema.Execute("{ withObjectArg(obj: { stringField: \"abc\" }) { StringField } }");

            Assert.AreEqual("abc", result.withObjectArg.StringField);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new RootQueryType(this.schema);
            var nestedTypeNonGeneric = new NestedNonGenericQueryType();
            var nestedType = new NestedQueryType(nestedTypeNonGeneric);

            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(nestedTypeNonGeneric);
            this.schema.AddKnownType(nestedType);
            this.schema.AddKnownType(new InputTestObjectType());

            this.schema.Query(rootType);
        }

        private class NestedNonGenericQueryType : GraphQLObjectType
        {
            public NestedNonGenericQueryType() : base("NestedNonGenericQueryType", "")
            {
                this.Field("text", (int id, string str) => $"{id} is from the parent and {str} is the current type");
            }
        }

        private class NestedQueryType : GraphQLObjectType<TestObject>
        {
            public NestedQueryType(NestedNonGenericQueryType nestedTypeNonGeneric) : base("NestedQueryType", "")
            {
                this.Field(instance => instance.Id);
                this.Field(instance => instance.StringField);
                this.Field("nested", () => nestedTypeNonGeneric);
            }
        }

        private class InputTestObjectType : GraphQLInputObjectType<TestObject>
        {
            public InputTestObjectType() : base("InputTestObjectType", "")
            {
                this.Field("stringField", instance => instance.StringField);
            }
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType(GraphQLSchema schema) : base("RootQueryType", "")
            {
                this.Field("nested", (int id) => new TestObject() { Id = id, StringField = "Test with id " + id });
                this.Field("withArray", (int[] ids) => ids.Count());
                this.Field("isNull", (int? nonMandatory) => !nonMandatory.HasValue);
                this.Field("withFloat", (float value) => value);
                this.Field("withList", (List<int> ids) => ids.Count());
                this.Field("withIEnumerable", (IEnumerable<int> ids) => ids.Count());
                this.Field("withObjectArg", (TestObject obj) => obj);
            }
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string StringField { get; set; }
        }
    }
}