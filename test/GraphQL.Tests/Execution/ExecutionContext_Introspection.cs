namespace GraphQL.Tests.Execution
{
    using GraphQL.Type;
    using Microsoft.CSharp.RuntimeBinder;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    [TestFixture]
    public class ExecutionContext_IntroSpection
    {
        private GraphQLSchema schema;

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsDefinedRootQueryType()
        {
            var result = GetSchemaFields();

            Assert.IsTrue(result.Any(e => e.name == "RootQueryType"));
        }

        private IEnumerable<dynamic> GetSchemaFields()
        {
            return (IEnumerable<dynamic>)this.schema.Execute(@"
            {
              __schema {
                types {
                  name
                }
              }
            }
            ").__schema.types;
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var rootType = new GraphQLObjectType("RootQueryType", "", this.schema);

            var type1 = new GraphQLObjectType("T1", "", this.schema);
            type1.Field("a", () => "1");
            type1.Field("b", () => 2);

            var type2 = new GraphQLObjectType("T2", "", this.schema);
            type2.Field("a", () => true);
            type2.Field("b", () => 1.2);

            rootType.Field("type1", () => type1);
            type1.Field("type1", () => type2);

            this.schema.SetRoot(rootType);
        }
    }
}
