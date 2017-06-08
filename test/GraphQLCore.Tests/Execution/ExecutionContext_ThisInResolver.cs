namespace GraphQLCore.Tests.Execution
{
    using NUnit.Framework;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Execution;
    using GraphQLCore.Type;

    public class ExecutionContext_ThisInResolver
    {
        private Schema schema;

        [SetUp]
        public void SetUp()
        {
            this.schema = new Schema();
        }

        [Test]
        public void Execute_ShouldSupplyExectionContextWithCorrectInstance()
        {
            var result = this.schema.Execute(@"
            {
                model {
                    number
                    numberPlusArgument(arg: 2)
                }
            }
            ");

            Assert.AreEqual(2, result.model.number);
            Assert.AreEqual(4, result.model.numberPlusArgument);
        }

        [Test]
        public void Execute_InstanceWillBeNullOnNonGenericGraphQlObject()
        {
            var result = this.schema.Execute(@"
            {
                isInstanceNull
            }
            ");

            Assert.AreEqual(true, result.isInstanceNull);
        }

        [Test]
        public void Execute_ThrowsErrorWhenTryingToCreateContextWithDifferentType()
        {
            var exception = Assert.Throws<GraphQLException>(() =>
            {
                var type = new ModelSchemaType();
                type.Field("test", (IContext<int> ctx) => true);
            });

            Assert.AreEqual(
                "Can't specify IContext of type \"Int32\" in GraphQLObjectType with type"+
                " \"GraphQLCore.Tests.Execution.ExecutionContext_ThisInResolver+Model\"", exception.Message);
        }

        public class Model
        {
            public int Number { get; set; }
        }

        public class ModelSchemaType : GraphQLObjectType<Model>
        {
            public ModelSchemaType() : base("Model", string.Empty)
            {
                this.Field("number", e => e.Number);
                this.Field("numberPlusArgument", (IContext<Model> ctx, int arg) => ctx.Instance.Number + arg);
            }
        }

        public class RootQueryType : GraphQLObjectType
        {
            public RootQueryType() : base("Query", string.Empty)
            {
                this.Field("isInstanceNull", (IContext<RootQueryType> ctx) => ctx.Instance == null);
                this.Field("model", () => new Model() { Number = 2 });
            }
        }

        public class Schema : GraphQLSchema
        {
            public Schema()
            {
                var queryType = new RootQueryType();
                this.AddKnownType(new ModelSchemaType());
                this.AddKnownType(queryType);

                this.Query(queryType);
            }
        }
    }
}
