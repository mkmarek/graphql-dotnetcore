namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Type.Scalar;
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
        public void Name_HasCorrectName()
        {
            Assert.AreEqual("Int", type.Name);
        }

        [SetUp]
        public void SetUp()
        {
            this.type = new GraphQLInt();
        }

        [Test]
        public void ToString_ReturnsName()
        {
            Assert.AreEqual("Int", type.ToString());
        }
    }
}