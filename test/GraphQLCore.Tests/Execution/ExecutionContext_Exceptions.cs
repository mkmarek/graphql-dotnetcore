namespace GraphQLCore.Tests.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Execution;
    using GraphQLCore.Type;
    using NUnit.Framework;
    using Validation;

    public class ExecutionContext_Exceptions
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_NullsOutExceptions()
        {
            var result = this.schema.Execute(@"
            {
                sync
                graphQLError
                graphQLErrorList
                rawError
            }
            ");

            Assert.AreEqual("foo", result.Data.sync);
            Assert.AreEqual(null, result.Data.graphQLError);

            var errors = result.Errors;
            Assert.AreEqual(3, errors.Count());

            ErrorAssert.AreEqual("Error getting graphQLError", errors.ElementAt(0), 4, 17, new[] { "graphQLError" });
            ErrorAssert.AreEqual("Error getting graphQLError", errors.ElementAt(1), 5, 17, new[] { "graphQLErrorList" });
            ErrorAssert.AreEqual("Error getting rawError", errors.ElementAt(2), 6, 17, new[] { "rawError" });
        }

        [Test]
        public void Execute_FullPathForNonNullFields()
        {
            var result = this.schema.Execute(@"
            query {
                nullableA {
                    aliasedA: nullableA {
                        nonNullA {
                            anotherA: nonNullA {
                                throws
                            }
                        }
                    }
                }
            }
            ");

            Assert.AreEqual(null, result.Data.nullableA.aliasedA);

            var errors = result.Errors;
            
            ErrorAssert.AreEqual("Catch me if you can", errors.Single(), 7, 33, new[] { "nullableA", "aliasedA", "nonNullA", "anotherA", "throws" });
        }

        [Test]
        public void Execute_FullPathForFieldsInList()
        {
            var result = this.schema.Execute(@"
            query {
                nullableAList {
                    foo
                    throwAlias : throws
                }
            }
            ");

            var nullableAList = (IList<object>)result.Data.nullableAList;
            Assert.AreEqual(4, nullableAList.Count);

            Assert.IsNotNull(nullableAList.ElementAt(0));
            Assert.IsNull(nullableAList.ElementAt(1));
            Assert.IsNotNull(nullableAList.ElementAt(2));
            Assert.IsNull(nullableAList.ElementAt(3));

            var errors = result.Errors;
            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("Catch me if you can", errors.ElementAt(0), 5, 21, new object[] { "nullableAList", 1, "throwAlias" });
            ErrorAssert.AreEqual("Catch me if you can", errors.ElementAt(1), 5, 21, new object[] { "nullableAList", 3, "throwAlias" });
        }

        [Test]
        public void Execute_FullPathForFieldsInNestedList()
        {
            var result = this.schema.Execute(@"
            query {
                nullableA {
                    nullableAList {
                        nullableAList {
                            nullableA {
                                throws
                            }
                        }
                    }
                }
            }
            ");

            dynamic nullableAList = result.Data.nullableA.nullableAList as IList<object>;

            Assert.AreEqual(4, nullableAList.Count);
            Assert.AreEqual("did not throw", nullableAList[0].nullableAList[0].nullableA.throws);
            Assert.AreEqual(null, nullableAList[3].nullableAList[3].nullableA);

            var errors = result.Errors.ToList();
            Assert.AreEqual(8, errors.Count);

            for (var i = 0; i < 4; i++)
            {
                ErrorAssert.AreEqual("Catch me if you can", errors[i * 2], 7, 33,
                    new object[] { "nullableA", "nullableAList", i, "nullableAList", 1, "nullableA", "throws" });
                ErrorAssert.AreEqual("Catch me if you can", errors[i * 2 + 1], 7, 33,
                    new object[] { "nullableA", "nullableAList", i, "nullableAList", 3, "nullableA", "throws" });
            }
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new QueryRootType();
            
            this.schema.AddKnownType(rootType);
            this.schema.AddKnownType(new A());
            this.schema.Query(rootType);
        }

        private class QueryRootType : GraphQLObjectType
        {
            public QueryRootType() : base("Query", "")
            {
                this.Field("sync", () => noError());
                this.Field("rawError", () => rawError());
                this.Field("graphQLError", () => graphQLError());
                this.Field("graphQLErrorList", () => new[] { noError(), graphQLError(), noError(), graphQLError() });

                this.Field("nullableA", () => new A(true));
                this.Field("nullableAList", () => new[] { new A(false), new A(true), new A(false), new A(true) });
            }

            private string noError() { return "foo"; }
            private string rawError() { throw new Exception("Error getting rawError"); }
            private string graphQLError() { throw new GraphQLException("Error getting graphQLError"); }
        }

        private class A : GraphQLObjectType
        {
            public A() : base("A", "")
            {
                this.AddFields();
            }

            public A(bool throws) : base("A", "")
            {
                this.AddFields(throws);
            }

            private void AddFields(bool throws = true)
            {
                this.Field("nullableA", () => new A(throws));
                this.Field("nullableAList", () => new[] { new A(false), new A(true), new A(false), new A(true) });
                this.Field<NonNullable<A>>("nonNullA", () => new A(throws));
                this.Field<NonNullable<string>>("throws", () => this.throws(throws));
                this.Field("foo", () => "bar");
            }

            private string throws(bool throws)
            {
                if (throws)
                    throw new GraphQLException("Catch me if you can");

                return "did not throw";
            }
        }
    }
}
