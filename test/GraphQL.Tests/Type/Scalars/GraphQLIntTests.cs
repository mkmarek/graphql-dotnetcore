namespace GraphQL.Tests.Type
{
    using GraphQL.Language.AST;
    using GraphQL.Type.Scalars;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLIntTests
    {
        private GraphQLInt type;

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Int", type.Name);
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Int", type.ToString());
        }

        [Test]
        public void Description_HasCorrectDescription()
        {
            Assert.AreEqual("The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.",
                type.Description);
        }

        [Test]
        public void GetFromAst_StringValue_ReturnsNull()
        {
            int? value = type.GetFromAst(new GraphQLValue<string>(ASTNodeKind.StringValue));

            Assert.IsNull(value);
        }

        [Test]
        public void GetFromAst_IntValue_ReturnsInt()
        {
            int? value = type.GetFromAst(new GraphQLValue<int>(ASTNodeKind.IntValue) { Value = 1 });

            Assert.AreEqual(1, value);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLInt(new GraphQL.Type.GraphQLSchema());
        }
    }
}
