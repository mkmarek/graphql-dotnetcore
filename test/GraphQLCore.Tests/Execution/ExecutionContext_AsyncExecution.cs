namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Diagnostics;
    using System.Threading.Tasks;

    [TestFixture]
    public class ExecutionContext_AsyncExecution
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_TwoAsyncActions_TakeAsLongAsSingleAsyncAction()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result = this.schema.Execute("{ async1, async2 }");
            sw.Stop();

            Assert.AreEqual(1, sw.ElapsedMilliseconds / 1000);
            Assert.AreEqual(42, result.Data.async1);
            Assert.AreEqual(42, result.Data.async2);
        }

        [Test]
        public void Execute_TwoNestedAsyncActions_TakeAsLongAsSingleAsyncAction()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result = this.schema.Execute("{ nested { async1, async2 } }");
            sw.Stop();

            Assert.AreEqual(1, sw.ElapsedMilliseconds / 1000);
            Assert.AreEqual(42, result.Data.nested.async1);
            Assert.AreEqual(42, result.Data.nested.async2);
        }

        [Test]
        public void Execute_TwoVeryNestedAsyncActions_TakeAsLongAsSingleAsyncAction()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result = this.schema.Execute("{ nested { nested { nested { async1, async2 }}} }");
            sw.Stop();

            Assert.AreEqual(1, sw.ElapsedMilliseconds / 1000);
            Assert.AreEqual(42, result.Data.nested.nested.nested.async1);
            Assert.AreEqual(42, result.Data.nested.nested.nested.async2);
        }

        [Test]
        public void Execute_TwoMutations_ShouldRunSynchronously()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var result = this.schema.Execute(
                "mutation { a, b }");
            sw.Stop();

            Assert.AreEqual(1, sw.ElapsedMilliseconds / 1000);
            Assert.AreEqual(42, result.Data.a);
            Assert.AreEqual(42, result.Data.b);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new RootQueryType(this.schema);
            var nestedType = new NestedQueryType();
            var mutation = new RootMutationType(this.schema);

            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(nestedType);
            this.schema.AddKnownType(mutation);

            this.schema.Query(rootType);
            this.schema.Mutation(mutation);
        }

        private class NestedQueryType : GraphQLObjectType
        {
            public NestedQueryType() : base("NestedQueryType", "")
            {
                this.Field("async1", () => this.GetValueAsync());
                this.Field("async2", () => this.GetValueAsync());
                this.Field("nested", () => new NestedQueryType());
            }

            private async Task<int> GetValueAsync()
            {
                await Task.Delay(1000);
                return 42;
            }
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType(GraphQLSchema schema) : base("RootQueryType", "")
            {
                this.Field("async1", () => this.GetValueAsync());
                this.Field("async2", () => this.GetValueAsync());
                this.Field("nested", () => new NestedQueryType());
            }

            private async Task<int> GetValueAsync()
            {
                await Task.Delay(1000);
                return 42;
            }
        }

        private class RootMutationType : GraphQLObjectType
        {
            private int value = -1;

            public RootMutationType(GraphQLSchema schema) : base("RootMutationType", "")
            {
                this.Field("a", () => this.Step1());
                this.Field("b", () => this.Step2());
            }

            private async Task<int> Step1()
            {
                await Task.Delay(500);
                this.value = 42;

                return this.value;
            }

            private async Task<int> Step2()
            {
                await Task.Delay(500);
                return this.value;
            }
        }
    }
}
