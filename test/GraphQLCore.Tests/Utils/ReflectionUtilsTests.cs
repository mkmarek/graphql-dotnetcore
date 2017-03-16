namespace GraphQLCore.Tests.Utils
{
    using NUnit.Framework;
    using System;
    using GraphQLCore.Utils;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class ReflectionUtilsTests
    {
        [Test]
        public void InputVariable_TypeInt32_ReturnsCorrectly()
        {
            Int64 inputVariable = 123456789;
            var result = ReflectionUtilities.ChangeValueType(inputVariable, typeof(Int32));

            Assert.AreEqual(inputVariable, result);
        }

        [Test]
        public void InputVariable_TypeInt32TooLarge_ReturnsNull()
        {
            Int64 inputVariable = Int64.MaxValue;
            var result = ReflectionUtilities.ChangeValueType(inputVariable, typeof(Int32));

            Assert.IsNull(result);
        }

        [Test]
        public void ChangeValueType_NonEnumerableToEnumerable_ReturnsNull()
        {
            var result = ReflectionUtilities.ChangeValueType(1, typeof(int[]));
            var result2 = ReflectionUtilities.ChangeValueType("test", typeof(List<string>));

            Assert.IsNull(result);
            Assert.IsNull(result2);
        }
    }
}
