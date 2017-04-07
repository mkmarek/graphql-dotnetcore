namespace GraphQLCore.Tests.Utils
{
    using GraphQLCore.Type;
    using GraphQLCore.Utils;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

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

        [Test]
        public void CreateNonNullableType_ReturnsCorrectType()
        {
            var result = ReflectionUtilities.CreateNonNullableType(typeof(string));
            var result2 = ReflectionUtilities.CreateNonNullableType(typeof(int));

            Assert.AreEqual(typeof(NonNullable<string>), result);
            Assert.AreEqual(typeof(int), result2);
        }

        [Test]
        public void IsNullable_ReturnsCorrectValue()
        {
            var result = ReflectionUtilities.IsNullable(typeof(int?));
            var result2 = ReflectionUtilities.IsNullable(typeof(int));

            Assert.IsTrue(result);
            Assert.IsFalse(result2);
        }
    }
}
