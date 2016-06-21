namespace GraphQL.Tests.Type.Introspection
{
    using GraphQL.Type;
    using GraphQL.Type.Introspection;
    using NUnit.Framework;

    public class SchemaTypeTests
    {
        private __Schema type;

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("__Schema", type.Name);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new __Schema(new GraphQLSchema());
        }
    }
}
