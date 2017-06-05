namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Type.Scalar;
    using NUnit.Framework;

    [TestFixture]
    public class IDTests
    {
        [Test]
        public void ID_FromStringToString_ReturnsCorrectValue()
        {
            ID id = "123";

            string idToString = id;
            Assert.AreEqual("123", idToString);
        }

        [Test]
        public void ID_FromIntToString_ReturnsCorrectValue()
        {
            ID id = 123;

            string idToString = id;
            Assert.AreEqual("123", idToString);
        }

        [Test]
        public void ID_FromInt64ToString_ReturnsCorrectValue()
        {
            ID id = 123456789012345;

            string idToString = id;
            Assert.AreEqual("123456789012345", idToString);
        }

        [Test]
        public void ID_EqualsIDWithSameValue()
        {
            ID id1 = 123;
            ID id2 = "123";

            Assert.AreEqual(id1, id2);
            Assert.AreEqual(id2, id1);
        }

        [Test]
        public void ID_EqualsStringWithSameValue()
        {
            ID id = "123";
            
            Assert.IsTrue(id == "123");
            Assert.IsTrue("123" == id);
        }

        [Test]
        public void ID_DoesNotEqualIDWithDifferentValue()
        {
            ID id1 = 123;
            ID id2 = "456";

            Assert.AreNotEqual(id1, id2);
        }

        [Test]
        public void ID_ReturnsNullWhenUnassigned()
        {
            ID id = null;

            Assert.IsNull((string)id);
        }
    }
}
