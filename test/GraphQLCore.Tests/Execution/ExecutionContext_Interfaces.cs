namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_Interfaces
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_ScalarBasedTruthyIncludeDirective_PrintsTheField()
        {
            dynamic result = this.schema.Execute("{ a, b @include(if: true) }");

            Assert.AreEqual("world", result.a);
            Assert.AreEqual("test", result.b);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();

            var nestedInteface = new GraphQLInterface("TestInterface", "", this.schema);
            nestedInteface.AddField<string>("a");

            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);
            rootType.Field("a", () => "world");
            rootType.Field("b", () => "test");

            var nestedType = new GraphQLObjectType("NestedQueryType", "", this.schema);
            nestedType.Field("a", () => "1");
            nestedType.Field("b", () => "2");
            nestedType.Implements(nestedInteface);

            rootType.Field("nested", () => nestedType);
            
            this.schema.SetRoot(rootType);
        }
    }
}
