namespace GraphQLCore.Tests.Execution
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionContext_FragmentDefinition
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_Fragment_ShouldResolveAndExecuteFragment()
        {
            var result = this.schema.Execute(@"
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

            Assert.AreEqual("1", result.Data.nested.a);
            Assert.AreEqual("2", result.Data.nested.b);
        }

        [Test]
        public void Execute_DuplicateFragments_ShouldNotReportAnyOtherExectionThenValidationException()
        {
            var result = this.schema.Execute(@"
            query fetch {
                nested {
                    ...frag
                }
            }

            fragment frag on NestedQueryType {
                a
            }

            fragment frag on NestedQueryType {
                b
            }
            ");

            var errors = result.Errors;

            Assert.IsNull(result.Data);
            Assert.AreEqual(1, errors.Count());
        }


        [Test]
        public void Execute_FragmentInFragment_ShouldResolveAndExecuteFragments()
        {
            var result = this.schema.Execute(@"
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

            Assert.AreEqual("1", result.Data.nested.a);
            Assert.AreEqual("2", result.Data.nested.b);
        }

        [Test]
        public void Execute_InlineFragment_ShouldResolveAndExecuteFragment()
        {
            var result = this.schema.Execute(@"
            query fetch {
                nested {
                    ... on NestedQueryType {
                        a
                        b
                    }
                }
            }
            ");

            Assert.AreEqual("1", result.Data.nested.a);
            Assert.AreEqual("2", result.Data.nested.b);
        }

        [Test]
        public void Execute_InlineFragmentInInlineFragment_ShouldResolveAndExecuteFragments()
        {
            var result = this.schema.Execute(@"
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

            Assert.AreEqual("1", result.Data.nested.a);
            Assert.AreEqual("2", result.Data.nested.b);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var nestedType = new NestedQueryType();
            var rootType = new RootQueryType(nestedType);

            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(nestedType);
            this.schema.Query(rootType);
        }

        private class NestedQueryType : GraphQLObjectType
        {
            public NestedQueryType() : base("NestedQueryType", "")
            {
                this.Field("a", () => "1");
                this.Field("b", () => "2");
            }
        }

        private class RootQueryType : GraphQLObjectType
        {
            public RootQueryType(NestedQueryType nested) : base("RootQueryType", "")
            {
                this.Field("nested", () => nested);
            }
        }
    }
}