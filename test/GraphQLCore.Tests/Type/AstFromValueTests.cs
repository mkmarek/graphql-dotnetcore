namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class AstFromValueTests
    {
        private ISchemaRepository schemaRepository;

        private static GraphQLBoolean graphQLBool = new GraphQLBoolean();
        private static GraphQLInt graphQLInt = new GraphQLInt();
        private static GraphQLFloat graphQLFloat = new GraphQLFloat();
        private static GraphQLString graphQLString = new GraphQLString();
        private static GraphQLID graphQLID = new GraphQLID();
        private static GraphQLLong graphQLLong = new GraphQLLong();

        private static GraphQLInputType nonNullBool = new GraphQLNonNull(graphQLBool);
        private static GraphQLEnumType myEnum = new MyEnumType();
        private static GraphQLInputObjectType inputObj = new MyInputObjectType();

        [Test]
        public void ConvertsBooleanValuesToASTs()
        {
            this.AreEqual(ASTNodeKind.BooleanValue, "true",
                graphQLBool.GetAstFromValue(true, schemaRepository));

            this.AreEqual(ASTNodeKind.BooleanValue, "false",
                graphQLBool.GetAstFromValue(false, schemaRepository));

            this.AreEqual(ASTNodeKind.NullValue,
                graphQLBool.GetAstFromValue(null, schemaRepository));

            this.AreEqual(ASTNodeKind.BooleanValue, "false",
                graphQLBool.GetAstFromValue(0, schemaRepository));

            this.AreEqual(ASTNodeKind.BooleanValue, "true",
                graphQLBool.GetAstFromValue(1, schemaRepository));

            this.AreEqual(ASTNodeKind.BooleanValue, "false",
                nonNullBool.GetAstFromValue(0, schemaRepository));

            Assert.IsNull(
                graphQLBool.GetAstFromValue("INVALID", schemaRepository));
        }

        [Test]
        public void ConvertsIntValuesToIntASTs()
        {
            this.AreEqual(ASTNodeKind.IntValue, "123",
                graphQLInt.GetAstFromValue(123.0f, schemaRepository));

            this.AreEqual(ASTNodeKind.IntValue, "10000",
                graphQLInt.GetAstFromValue(1e4, schemaRepository));

            Assert.IsNull(
                graphQLInt.GetAstFromValue(123.5, schemaRepository));

            Assert.IsNull(
                graphQLInt.GetAstFromValue(1e40, schemaRepository));
        }

        [Test]
        public void ConvertsFloatValuesToIntOrFloatASTs()
        {
            this.AreEqual(ASTNodeKind.IntValue, "123",
                graphQLFloat.GetAstFromValue(123.0, schemaRepository));

            this.AreEqual(ASTNodeKind.FloatValue, "123.5",
                graphQLFloat.GetAstFromValue(123.5, schemaRepository));

            this.AreEqual(ASTNodeKind.IntValue, "10000",
                graphQLFloat.GetAstFromValue(1e4, schemaRepository));

            this.AreEqual(ASTNodeKind.FloatValue, "1e+40",
                graphQLFloat.GetAstFromValue(1e40, schemaRepository));

            Assert.IsNull(
                graphQLFloat.GetAstFromValue("INVALID", schemaRepository));
        }

        [Test]
        public void ConvertsStringValuesToStringASTs()
        {
            this.AreEqual(ASTNodeKind.StringValue, "hello",
                graphQLString.GetAstFromValue("hello", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "VALUE",
                graphQLString.GetAstFromValue("VALUE", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "VA\\nLUE",
                graphQLString.GetAstFromValue("VA\nLUE", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "123",
                graphQLString.GetAstFromValue(123, schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "false",
                graphQLString.GetAstFromValue(false, schemaRepository));

            this.AreEqual(ASTNodeKind.NullValue,
                graphQLString.GetAstFromValue(null, schemaRepository));
        }

        [Test]
        public void ConvertsIDValuesToIntOrStringASTs()
        {
            this.AreEqual(ASTNodeKind.StringValue, "hello",
                graphQLID.GetAstFromValue("hello", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "VALUE",
                graphQLID.GetAstFromValue("VALUE", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "VA\\nLUE",
                graphQLID.GetAstFromValue("VA\nLUE", schemaRepository));

            this.AreEqual(ASTNodeKind.IntValue, "123",
                graphQLID.GetAstFromValue("123", schemaRepository));

            this.AreEqual(ASTNodeKind.StringValue, "false",
                graphQLID.GetAstFromValue(false, schemaRepository));

            this.AreEqual(ASTNodeKind.NullValue,
                graphQLID.GetAstFromValue(null, schemaRepository));
        }

        [Test]
        public void ConvertsLongValuesToIntASTs()
        {
            this.AreEqual(ASTNodeKind.IntValue, "123",
                graphQLLong.GetAstFromValue(123.0f, schemaRepository));

            this.AreEqual(ASTNodeKind.IntValue, "10000",
                graphQLLong.GetAstFromValue(1e4, schemaRepository));

            this.AreEqual(ASTNodeKind.IntValue, "10000000000000",
                graphQLLong.GetAstFromValue(10000000000000, schemaRepository));

            Assert.IsNull(
                graphQLLong.GetAstFromValue(123.5, schemaRepository));

            Assert.IsNull(
                graphQLLong.GetAstFromValue(1e40, schemaRepository));

            Assert.IsNull(
                graphQLLong.GetAstFromValue("foo", schemaRepository));
        }

        [Test]
        public void DoesNotConvertNonNullValuesToNullValue()
        {
            Assert.IsNull(nonNullBool.GetAstFromValue(null, schemaRepository));
        }

        [Test]
        public void ConvertsStringValuesToEnumASTsIfPossible()
        {
            this.schemaRepository.AddKnownType(myEnum);

            this.AreEqual(ASTNodeKind.EnumValue, "HELLO",
                myEnum.GetAstFromValue("HELLO", schemaRepository));

            Assert.IsNull(
                myEnum.GetAstFromValue("hello", schemaRepository));

            Assert.IsNull(
                myEnum.GetAstFromValue("VALUE", schemaRepository));
        }

        [Test]
        public void ConvertsArrayValuesToListASTs()
        {
            this.schemaRepository.AddKnownType(myEnum);

            this.AreEqual(ASTNodeKind.ListValue, new[]
                {
                    new GraphQLScalarValue(ASTNodeKind.StringValue) { Value = "FOO" },
                    new GraphQLScalarValue(ASTNodeKind.StringValue) { Value = "BAR" },
                },
                new GraphQLList(graphQLString).GetAstFromValue(new[] { "FOO", "BAR" }, schemaRepository));

            this.AreEqual(ASTNodeKind.ListValue, new[]
                {
                    new GraphQLScalarValue(ASTNodeKind.EnumValue) { Value = "HELLO" },
                    new GraphQLScalarValue(ASTNodeKind.EnumValue) { Value = "GOODBYE" },
                },
                new GraphQLList(myEnum).GetAstFromValue(new[] { "HELLO", "GOODBYE" }, schemaRepository));
        }

        [Test]
        public void ConvertsListSingletons()
        {
            this.AreEqual(ASTNodeKind.StringValue, "FOO",
                new GraphQLList(graphQLString).GetAstFromValue("FOO", schemaRepository));
        }

        [Test]
        public void ConvertsInputObjects()
        {
            this.schemaRepository.AddKnownType(myEnum);
            this.schemaRepository.AddKnownType(inputObj);

            var expected = new GraphQLObjectValue()
            {
                Fields = new[]
                {
                    new GraphQLObjectField()
                    {
                        Name = new GraphQLName() { Value = "foo" },
                        Value = new GraphQLScalarValue(ASTNodeKind.IntValue) { Value = "3" }
                    },
                    new GraphQLObjectField()
                    {
                        Name = new GraphQLName() { Value = "bar" },
                        Value = new GraphQLScalarValue(ASTNodeKind.EnumValue) { Value = "HELLO" }
                    }
                }
            };

            var actual = inputObj.GetAstFromValue(new MyInput() { Foo = 3, Bar = MyEnum.HELLO }, schemaRepository);

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaRepository = new SchemaRepository();
        }

        private void AreEqual(ASTNodeKind expectedKind, dynamic actual)
        {
            Assert.AreEqual(expectedKind, actual.Kind);
        }

        private void AreEqual(ASTNodeKind expectedKind, object expectedValue, dynamic actual)
        {
            Assert.AreEqual(expectedKind, actual.Kind);
            Assert.AreEqual(expectedValue, actual.Value);
        }

        private void AreEqual(ASTNodeKind expectedKind, IList<dynamic> expectedValues, dynamic actual)
        {
            Assert.AreEqual(expectedKind, actual.Kind);

            for (var i = 0; i < expectedValues.Count; i++)
            {
                Assert.AreEqual(expectedValues[i].Kind, actual.Values[i].Kind);
                Assert.AreEqual(expectedValues[i].Value, actual.Values[i].Value);
            }
        }

        public enum MyEnum { HELLO, GOODBYE, COMPLEX }

        public class MyEnumType : GraphQLEnumType<MyEnum>
        {
            public MyEnumType() : base("MyEnum", "")
            {
            }
        }

        public class MyInput
        {
            public float? Foo;
            public MyEnum? Bar;
        }

        public class MyInputObjectType : GraphQLInputObjectType<MyInput>
        {
            public MyInputObjectType() : base("MyInput", "")
            {
                this.Field("foo", e => e.Foo);
                this.Field("bar", e => e.Bar);
            }
        }
    }
}
