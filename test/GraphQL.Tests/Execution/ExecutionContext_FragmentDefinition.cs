namespace GraphQL.Tests.Execution
{
    using GraphQL.Type;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_FragmentDefinition
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_Fragment_ShouldResolveAndExecuteFragment()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                nested {
                    ...frag 
                }
            }

            fragment frag on NestedQueryType {
                a
                b
            }
            ");

            Assert.AreEqual("1", result.nested.a);
            Assert.AreEqual("2", result.nested.b);
        }

        [Test]
        public void Execute_FragmentInFragment_ShouldResolveAndExecuteFragments()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                ...frag1
            }

            fragment frag1 on RootQueryType {
                nested {
                    ...frag2
                }
            }

            fragment frag2 on NestedQueryType {
                a
                b
            }
            ");

            Assert.AreEqual("1", result.nested.a);
            Assert.AreEqual("2", result.nested.b);
        }


        [SetUp]
        public void SetUp()
        {
            var rootType = new GraphQLObjectType("RootQueryType", "");

            var nestedType = new GraphQLObjectType("NestedQueryType", "");
            nestedType.AddField("a", () => "1");
            nestedType.AddField("b", () => "2");

            rootType.AddField("nested", () => nestedType);

            this.schema = new GraphQLSchema(rootType);
        }
    }
}
