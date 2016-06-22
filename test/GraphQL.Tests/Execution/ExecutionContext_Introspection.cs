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
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "RootQueryType"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsT1()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "T1"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsT2()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "T2"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_Contains__Schema()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "__Schema"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_Contains__Type()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "__Type"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsInt()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "Int"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsBoolean()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "Boolean"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsString()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "String"));
        }

        [Test]
        public void Execute_IntrospectingSchemaTypeNames_ContainsFloat()
        {
            var result = GetSchemaFields();
            Assert.IsNotNull(result.SingleOrDefault(e => e.name == "Float"));
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
