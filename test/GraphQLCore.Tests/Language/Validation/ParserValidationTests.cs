namespace GraphQLCore.Tests.Language.Validation
{
    using Exceptions;
    using GraphQLCore.Language;
    using NUnit.Framework;

    [TestFixture]
    public class ParserValidationTests
    {
        [Test]
        public void Parse_MissingEndingBrace_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("{"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:2) Expected Name, found EOF
1: {
    ^
", exception.Message);
        }

        [Test]
        public void Parse_MissingFragmentType_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source(@"{ ...MissingOn }
fragment MissingOn Type"))));

            Assert.AreEqual(@"Syntax Error GraphQL (2:20) Expected "+"\"on\""+ @", found Name " + "\"Type\"" + @"
1: { ...MissingOn }
2: fragment MissingOn Type
                      ^
", exception.Message);
        }

        [Test]
        public void Parse_MissingFieldNameWhenAliasing_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("{ field: {} }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:10) Expected Name, found {
1: { field: {} }
            ^
", exception.Message);
        }

        [Test]
        public void Parse_UnknownOperation_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("notanoperation Foo { field }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:1) Unexpected Name "+"\"notanoperation\"" + @"
1: notanoperation Foo { field }
   ^
", exception.Message);
        }

        [Test]
        public void Parse_LonelySpread_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("..."))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:1) Unexpected ...
1: ...
   ^
", exception.Message);
        }

        [Test]
        public void Parse_FragmentInvalidOnName_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("fragment on on on { on }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:10) Unexpected Name "+ "\"on\"" + @"
1: fragment on on on { on }
            ^
", exception.Message);
        }

        [Test]
        public void Parse_InvalidDefaultValue_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("query Foo($x: Complex = { a: { b: [ $var ] } }) { field }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:37) Unexpected $
1: query Foo($x: Complex = { a: { b: [ $var ] } }) { field }
                                       ^
", exception.Message);
        }

        [Test]
        public void Parse_InvalidFragmentNameInSpread_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("{ ...on }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:9) Expected Name, found }
1: { ...on }
           ^
", exception.Message);
        }

        [Test]
        public void Parse_InvalidNullAsValue_ThrowsExceptionWithCorrectMessage()
        {
            var exception = Assert.Throws<GraphQLSyntaxErrorException>(
                new TestDelegate(() => new Parser(new Lexer()).Parse(new Source("{ fieldWithNullableStringInput(input: null) }"))));

            Assert.AreEqual(@"Syntax Error GraphQL (1:39) Unexpected Name " + "\"null\"" + @"
1: { fieldWithNullableStringInput(input: null) }
                                         ^
", exception.Message);
        }
    }
}
