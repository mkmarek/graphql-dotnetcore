namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class NonNullableTests
    {
        [Test]
        public void NonNullable_WithNullArgument_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var x = new NonNullable<object>(null);
            });
        }

        [Test]
        public void NonNullable_WithNullValue_GetValue_Throws()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var x = new NonNullable<object>();
                var y = x.Value;
            });
        }

        [Test]
        public void NonNullables_WithSameValue_Equal()
        {
            var test = Test.Create();
            var test2 = test;

            var first = new NonNullable<object>(test);
            var second = new NonNullable<object>(test);

            Assert.AreEqual(first, second);
            Assert.IsTrue(NonNullable.Equals(first, second));
        }

        [Test]
        public void NonNullable_WithBaseObject_Equal()
        {
            var second = Test.Create();

            var first = new NonNullable<object>(second);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void NonNullable_WorksWithUnderlyingOperators()
        {
            var foo = new NonNullable<string>("foo");
            var bar = new NonNullable<string>("bar");

            var foobar = foo + bar;

            Assert.AreEqual("foobar", foobar);
        }

        [Test]
        public void NonNullable_GetUnderlyingType_ReturnsCorrectUnderlyingType()
        {
            var type = typeof(NonNullable<Test>);
            var wrongType = typeof(string[]);

            var result = NonNullable.GetUnderlyingType(type);
            var result2 = NonNullable.GetUnderlyingType(wrongType);

            Assert.AreEqual(typeof(Test), result);
            Assert.AreEqual(null, result2);
        }

        [Test]
        public void NonNullable_GetUnderlyingType_WithNullType_ThrowsError()
        {
            Type type = null;

            Assert.Throws<ArgumentNullException>(() => NonNullable.GetUnderlyingType(type), "Value cannot be null.\nParameter name: nonNullableType");
        }

        [Test]
        public void NonNullable_FromNullable_IsOfCorrectType()
        {
            var result = (NonNullable<string>)"foo";

            Assert.IsInstanceOf(typeof(NonNullable<string>), result);
        }

        [Test]
        public void NonNullable_GetValue_ReturnsCorrectValue()
        {
            var value = Test.Create();
            INonNullable test = (NonNullable<Test>)value;

            var result = test.GetValue();

            Assert.AreEqual(value, result);
        }

        [Test]
        public void NonNullable_GetHashCode_ReturnsHashcodeForValue()
        {
            var test = Test.Create();
            var nonNullTest = new NonNullable<Test>(test);

            Assert.AreEqual(test.GetHashCode(), nonNullTest.GetHashCode());
        }

        [Test]
        public void NonNullable_ToString_ReturnsCorrectValue()
        {
            var test = Test.Create();
            var nonNullTest = new NonNullable<Test>(test);

            Assert.AreEqual(test.ToString(), nonNullTest.ToString());
        }
    }

    public class Test
    {
        public int Int { get; set; }
        public string String { get; set; }
        public bool? NullableBool { get; set; }

        public static Test Create()
        {
            return new Test()
            {
                Int = 1,
                NullableBool = null
            };
        }
    }
}
