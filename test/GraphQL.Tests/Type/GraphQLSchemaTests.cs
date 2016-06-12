namespace GraphQL.Tests.Type
{
    using GraphQL.Type;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLSchemaTests
    {
        private GraphQLSchema schema;

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
        }
    }
}
