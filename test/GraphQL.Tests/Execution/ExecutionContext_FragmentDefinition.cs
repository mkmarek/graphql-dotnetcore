namespace GraphQL.Tests.Execution
{
    using GraphQL.Type;
    using Microsoft.CSharp.RuntimeBinder;
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
        public void Execute_InlineFragment_ShouldResolveAndExecuteFragment()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                nested {
                    ... on NestedQueryType {
                        a
                        b
                    } 
                }
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

        [Test]
        public void Execute_InlineFragmentInInlineFragment_ShouldResolveAndExecuteFragments()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                ... on RootQueryType {
                    nested {
                        ... on NestedQueryType {
                            a
                            b
                        }
                    }
                }
            }
            ");

            Assert.AreEqual("1", result.nested.a);
            Assert.AreEqual("2", result.nested.b);
        }

        [Test]
        public void Execute_InlineFragmentDifferentType_ShouldntResolveFragment()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                ... on NestedQueryType {
                    a
                    b
                }
            }
            ");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string x = result.a; }));
            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string x = result.b; }));
        }

        [Test]
        public void Execute_FragmentDefinitionDifferentType_ShouldntResolveFragment()
        {
            dynamic result = this.schema.Execute(@"
            query fetch {
                ... frag
            }

            fragment frag on NestedQueryType {
                a
                b
            }
            ");

            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string x = result.a; }));
            Assert.Throws<RuntimeBinderException>(new TestDelegate(() => { string x = result.b; }));
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);

            var nestedType = new GraphQLObjectType("NestedQueryType", "", this.schema);
            nestedType.Field("a", () => "1");
            nestedType.Field("b", () => "2");

            rootType.Field("nested", () => nestedType);

            this.schema.SetRoot(rootType);
        }
    }
}
