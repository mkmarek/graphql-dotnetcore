namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Scalars;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLIntTests
    {
        private GraphQLInt type;

        [Test]
        public void Description_HasCorrectDescription()
        {
            Assert.AreEqual("The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.",
                type.Description);
        }

        [Test]
        public void GetFromAst_IntValue_ReturnsInt()
        {
            int? value = type.GetFromAst(new GraphQLValue<int>(ASTNodeKind.IntValue) { Value = 1 });

            Assert.AreEqual(1, value);
        }

        [Test]
        public void GetFromAst_StringValue_ReturnsNull()
        {
            int? value = type.GetFromAst(new GraphQLValue<string>(ASTNodeKind.StringValue));

            Assert.IsNull(value);
        }

        [Test]
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Int", type.Name);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLInt(new GraphQLCore.Type.GraphQLSchema());
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Int", type.ToString());
        }
    }
}