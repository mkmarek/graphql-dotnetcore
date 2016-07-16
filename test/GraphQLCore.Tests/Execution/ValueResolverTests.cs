namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Translation;
    using NSubstitute;
    using NUnit.Framework;
    using System.Dynamic;

    [TestFixture]
    public class ValueResolverTests
    {
        private ITypeTranslator typeTranslator;
        private ValueResolver valueResolver;
        private IVariableResolver variableResolver;

        [Test]
        public void GetValue_GraphQLObjectValue_ReturnsExpandoObject()
        {
            var value = new GraphQLObjectValue();

            var result = this.valueResolver.GetValue(value);

            Assert.IsInstanceOf<ExpandoObject>(result);
        }

        [Test]
        public void GetValue_GraphQLObjectValueWithIntField_ReturnsExpandoObjectWithIntegerField()
        {
            var literalValue = new GraphQLValue<int>(ASTNodeKind.IntValue);
            this.typeTranslator.GetLiteralValue(literalValue).Returns(123);

            var value = new GraphQLObjectValue()
            {
                Fields = new GraphQLObjectField[] {
                     GetObjectField(literalValue)
                }
            };

            var result = this.valueResolver.GetValue(value) as dynamic;

            Assert.AreEqual(123, result.fieldA);
        }

        [SetUp]
        public void SetUp()
        {
            this.variableResolver = Substitute.For<IVariableResolver>();
            this.typeTranslator = Substitute.For<ITypeTranslator>();
            this.valueResolver = new ValueResolver(this.variableResolver, this.typeTranslator);
        }

        private static GraphQLObjectField GetObjectField(GraphQLValue value)
        {
            return new GraphQLObjectField()
            {
                Name = new GraphQLName() { Value = "fieldA" },
                Value = value
            };
        }
    }
}
