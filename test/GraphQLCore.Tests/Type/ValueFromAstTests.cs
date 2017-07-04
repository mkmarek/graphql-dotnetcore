namespace GraphQLCore.Tests.Type
{
    using GraphQLCore.Execution;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    [TestFixture]
    public class ValueFromAstTests
    {
        private ISchemaRepository schemaRepository;
        private Parser parser = new Parser(new Lexer());

        private static GraphQLBoolean graphQLBool = new GraphQLBoolean();
        private static GraphQLInt graphQLInt = new GraphQLInt();
        private static GraphQLFloat graphQLFloat = new GraphQLFloat();
        private static GraphQLString graphQLString = new GraphQLString();
        private static GraphQLID graphQLID = new GraphQLID();
        private static GraphQLLong graphQLLong = new GraphQLLong();

        private static GraphQLInputType nonNullBool = new GraphQLNonNull(graphQLBool);
        private static GraphQLInputType listOfBool = new GraphQLList(graphQLBool);
        private static GraphQLInputType nonNullListOfBool = new GraphQLNonNull(new GraphQLList(graphQLBool));
        private static GraphQLInputType listOfNonNullBool = new GraphQLList(new GraphQLNonNull(graphQLBool));
        private static GraphQLInputType nonNullListOfNonNullBool = new GraphQLNonNull(new GraphQLList(new GraphQLNonNull(graphQLBool)));

        private static GraphQLInputType testInputObj = new TestInputObjectType();

        [Test]
        public void ConvertsAccordingToInputCoercionRules()
        {
            this.AreEqual(true, graphQLBool, "true");
            this.AreEqual(false, graphQLBool, "false");
            this.AreEqual(123, graphQLInt, "123");
            this.AreEqual(123, graphQLFloat, "123");
            this.AreEqual(123.456f, graphQLFloat, "123.456");
            this.AreEqual(9999999999999999999999999999999999999f, graphQLFloat, "9999999999999999999999999999999999999");
            this.AreEqual("abc123", graphQLString, "\"abc123\"");
            this.AreEqual("123456", graphQLID, "123456");
            this.AreEqual("123456", graphQLID, "\"123456\"");
            this.AreEqual(123, graphQLLong, "123");
            this.AreEqual(1234567890123456789, graphQLLong, "1234567890123456789");
        }

        [Test]
        public void DoesNotConvertWhenInputCoercionRulesRejectAValue()
        {
            this.IsInvalid(graphQLBool, "123");
            this.IsInvalid(graphQLInt, "123.456");
            this.IsInvalid(graphQLInt, "true");
            this.IsInvalid(graphQLInt, "\"123\"");
            this.IsInvalid(graphQLInt, "9999999999999999999999999999999999999");
            this.IsInvalid(graphQLFloat, "\"123\"");
            this.IsInvalid(graphQLFloat, "99999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999");
            this.IsInvalid(graphQLString, "123");
            this.IsInvalid(graphQLString, "true");
            this.IsInvalid(graphQLID, "123.456");
            this.IsInvalid(graphQLLong, "123.456");
            this.IsInvalid(graphQLLong, "true");
            this.IsInvalid(graphQLLong, "\"123\"");
            this.IsInvalid(graphQLLong, "12345678901234567890");
            this.IsInvalid(graphQLLong, "9999999999999999999999999999999999999");
        }

        [Test]
        public void ConvertsEnumValuesAccordingToInputCoercionRules()
        {
            var testEnum = new GraphQLEnumType<TestEnum>("TestColor", "");
            this.schemaRepository.AddKnownType(testEnum);

            this.AreEqual(TestEnum.RED, testEnum, "RED");
            this.AreEqual(TestEnum.BLUE, testEnum, "BLUE");
            this.AreEqual(TestEnum.NULL, testEnum, "NULL");
            this.AreEqual(null, testEnum, "null");

            this.IsInvalid(testEnum, "3");
            this.IsInvalid(testEnum, "\"BLUE\"");
            this.IsInvalid(testEnum, "UNDEFINED");
        }

        [Test]
        public void CoercesToNullUnlessNonNull()
        {
            this.AreEqual(null, graphQLBool, "null");
            this.IsInvalid(nonNullBool, "null");
        }

        [Test]
        public void CoercesListsOfValues()
        {
            this.AreEqual(new[] { true }, listOfBool, "true");
            this.AreEqual(new[] { true, false }, listOfBool, "[true, false]");
            this.AreEqual(new bool?[] { true, null }, listOfBool, "[true, null]");
            this.AreEqual(null, listOfBool, "null");

            this.IsInvalid(listOfBool, "123");
            this.IsInvalid(listOfBool, "[true, 123]");
            this.IsInvalid(listOfBool, "{ true: true }");
        }

        [Test]
        public void CoercesNonNullListsOfValues()
        {
            this.AreEqual(new[] { true }, nonNullListOfBool, "true");
            this.AreEqual(new[] { true, false }, nonNullListOfBool, "[true, false]");
            this.AreEqual(new bool?[] { true, null }, nonNullListOfBool, "[true, null]");

            this.IsInvalid(nonNullListOfBool, "123");
            this.IsInvalid(nonNullListOfBool, "null");
            this.IsInvalid(nonNullListOfBool, "[true, 123]");
        }

        [Test]
        public void CoercesListsOfNonNullValues()
        {
            this.AreEqual(new[] { true }, listOfNonNullBool, "true");
            this.AreEqual(null, listOfNonNullBool, "null");
            this.AreEqual(new[] { true, false }, listOfNonNullBool, "[true, false]");

            this.IsInvalid(listOfNonNullBool, "123");
            this.IsInvalid(listOfNonNullBool, "[true, 123]");
            this.IsInvalid(listOfNonNullBool, "[true, null]");
        }

        [Test]
        public void CoercesNonNullListsOfNonNullValues()
        {
            this.AreEqual(new[] { true }, nonNullListOfNonNullBool, "true");
            this.AreEqual(new[] { true, false }, nonNullListOfNonNullBool, "[true, false]");

            this.IsInvalid(nonNullListOfNonNullBool, "123");
            this.IsInvalid(nonNullListOfNonNullBool, "null");
            this.IsInvalid(nonNullListOfNonNullBool, "[true, 123]");
            this.IsInvalid(nonNullListOfNonNullBool, "[true, null]");
        }

        [Test]
        public void CoercesInputObjectsAccordingToInputCoercionRules()
        {
            this.AreEqual(null, testInputObj, "null");
            this.AreEqual(new TestInput() { Int = 123, RequiredBool = false }, testInputObj, "{ int: 123, requiredBool: false }");
            this.AreEqual(new TestInput() { Int = 42, Bool = true, RequiredBool = false }, testInputObj, "{ bool: true, requiredBool: false }");

            this.IsInvalid(testInputObj, "123");
            this.IsInvalid(testInputObj, "[]");
            this.IsInvalid(testInputObj, "{ int: true, requiredBool: true }");
            this.IsInvalid(testInputObj, "{ requiredBool: null }");
            this.IsInvalid(testInputObj, "{ bool: true }");
        }

        [Test]
        public void AcceptsVariableValuesAssumingAlreadyCoerced()
        {
            this.AreEqual(null, graphQLBool, "$var");

            this.MockVariable("Boolean", "var", true);
            this.AreEqual(true, graphQLBool, "$var");

            this.MockVariable("Boolean", "var", null);
            this.AreEqual(null, graphQLBool, "$var");
        }

        [Test]
        public void AssertsVariablesAreProvidedAsItemsInLists()
        {
            this.AreEqual(new bool?[] { null }, listOfBool, "[ $foo ]");
            this.IsInvalid(listOfNonNullBool, "[ $foo ]");

            this.MockVariable("Boolean", "foo", true);
            this.AreEqual(new[] { true }, listOfNonNullBool, "[ $foo ]");

            this.MockVariable("Boolean", "foo", true);
            this.AreEqual(true, listOfNonNullBool, "$foo");

            this.MockVariable("Boolean", "foo", new bool?[] { true });
            this.AreEqual(new[] { true }, listOfNonNullBool, "$foo");
        }

        [Test]
        public void OmitsInputObjectFieldsForUnprovidedVariables()
        {
            this.AreEqual(new TestInput()
            {
                Int = 42,
                RequiredBool = true
            }, testInputObj, "{ int: $foo, bool: $foo, requiredBool: true }");
            
            this.IsInvalid(testInputObj, "{ requiredBool: $foo }");

            this.MockVariable("Boolean", "foo", true);
            this.AreEqual(new TestInput()
            {
                Int = 42,
                RequiredBool = true
            }, testInputObj, "{ requiredBool: $foo }");
        }

        private void MockVariable(string typeName, string variableName, object variableValue)
        {
            var variables = new ExpandoObject() as IDictionary<string, object>;
            variables.Add(variableName, variableValue);
            var variableDefinitions = new List<GraphQLVariableDefinition>()
            {
                new GraphQLVariableDefinition()
                {
                    Type = new GraphQLNamedType()
                    {
                        Name = new GraphQLName()
                        {
                            Value = typeName
                        }
                    },
                    Variable = new GraphQLVariable()
                    {
                        Name = new GraphQLName()
                        {
                            Value = variableName
                        }
                    }
                }
            };

            this.schemaRepository.VariableResolver = new VariableResolver(variables, this.schemaRepository, variableDefinitions);
        }

        private void AreEqual(object expected, GraphQLInputType type, string value)
        {
            var result = this.GetFromAst(type, value);
            
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(expected, result.Value);
        }

        private void IsInvalid(GraphQLInputType type, string value)
        {
            var result = this.GetFromAst(type, value);

            Assert.IsFalse(result.IsValid);
        }

        private Result GetFromAst(GraphQLInputType type, string value)
        {
            var graphQLValue = parser.ParseValue(new Source(value));

            return type.GetFromAst(graphQLValue, this.schemaRepository);
        }

        [SetUp]
        public void SetUp()
        {
            this.schemaRepository = new SchemaRepository();
            this.schemaRepository.VariableResolver = new VariableResolver(new ExpandoObject(), this.schemaRepository,
                Enumerable.Empty<GraphQLVariableDefinition>());
        }
    }

    public enum TestEnum {
        RED,
        GREEN,
        BLUE,
        NULL
    }

    public class TestInput
    {
        public int? Int { get; set; }
        public bool? Bool { get; set; }
        public bool RequiredBool { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as TestInput;
            
            return
                this.Int == other.Int &&
                this.Bool == other.Bool &&
                this.RequiredBool == other.RequiredBool;
        }
    }

    public class TestInputObjectType : GraphQLInputObjectType<TestInput>
    {
        public TestInputObjectType() : base("TestInput", "")
        {
            this.Field("int", e => e.Int).WithDefaultValue(42);
            this.Field("bool", e => e.Bool);
            this.Field("requiredBool", e => e.RequiredBool);
        }
    }
}
