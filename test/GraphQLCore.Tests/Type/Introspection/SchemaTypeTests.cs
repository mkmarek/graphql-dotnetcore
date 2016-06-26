namespace GraphQLCore.Tests.Type.Introspection
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Introspection;
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