namespace GraphQLCore.Tests.Utils
{
    using GraphQLCore.Utils;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class StringUtilsTests
    {
        [Test]
        public void SuggestionList_EmptyInput_ReturnsResults()
        {
            var results = StringUtils.SuggestionList(
                "",
                new string[] { "a" });

            Assert.AreEqual("a", results.ElementAt(0));
        }

        [Test]
        public void SuggestionList_NoOptions_ReturnsEmptyArray()
        {
            var results = StringUtils.SuggestionList(
                "",
                new string[] {});

            Assert.IsEmpty(results);
        }

        [Test]
        public void SuggestionList_ReturnsOptionsBasedOnSimilarity()
        {
            var results = StringUtils.SuggestionList(
                "abc",
                new string[] { "a", "ab", "abc" });

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual("abc", results.ElementAt(0));
            Assert.AreEqual("ab", results.ElementAt(1));
        }

        [Test]
        public void QuotedList_SingleItem_ReturnsSingleQuotedItem()
        {
            var result = StringUtils.QuotedOrList(
                new string[] { "a" });

            Assert.AreEqual("\"a\"", result);
        }

        [Test]
        public void QuotedList_TwoItems_ReturnsTwoItemList()
        {
            var result = StringUtils.QuotedOrList(
                new string[] { "a", "b" });

            Assert.AreEqual("\"a\" or \"b\"", result);
        }

        [Test]
        public void QuotedList_ThreeItems_ReturnsCommaSeparatedManyItemList()
        {
            var result = StringUtils.QuotedOrList(
                new string[] { "a", "b", "c" });

            Assert.AreEqual("\"a\", \"b\", or \"c\"", result);
        }

        [Test]
        public void QuotedList_SixItems_LimitsToFiveItems()
        {
            var result = StringUtils.QuotedOrList(
                new string[] { "a", "b", "c", "d", "e", "f" });

            Assert.AreEqual("\"a\", \"b\", \"c\", \"d\", or \"e\"", result);
        }

        [Test]
        public void Dedent_WorksWithoutInterpolation()
        {
            var result = StringUtils.Dedent(
                @"first
                  second
                  third");

            Assert.AreEqual(
@"first
second
third".Replace("\r", string.Empty),
            result);
        }

        [Test]
        public void Dedent_WorksWithInterpolation()
        {
            var result = StringUtils.Dedent(
                $@"first {"line"}
                {"second"}
                third");

            Assert.AreEqual(
@"first line
second
third".Replace("\r", string.Empty),
            result);
        }

        [Test]
        public void Dedent_WorksWithSuppressedNewlines()
        {
            var result = StringUtils.Dedent(
                $@"first \
                {"second"}
                third");

            Assert.AreEqual(
@"first second
third".Replace("\r", string.Empty),
            result);
        }

        [Test]
        public void Dedent_WorksWithBlankFirstLine()
        {
            var result = StringUtils.Dedent(@"
                Some text that I might want to indent:
                  * reasons
                  * fun
                That's all.
            ");

            Assert.AreEqual(
@"Some text that I might want to indent:
  * reasons
  * fun
That's all.".Replace("\r", string.Empty),
            result);
        }

        [Test]
        public void Dedent_WorksWithMultipleBlankFirstLines()
        {
            var result = StringUtils.Dedent(@"

            first
            second
            third
            ");

            Assert.AreEqual(
@"first
second
third".Replace("\r", string.Empty),
                result);
        }

        [Test]
        public void Dedent_WorksWithRemovingSameNumberOfSpaces()
        {
            var result = StringUtils.Dedent(@"
                first
                   second
                      third
            ");

            Assert.AreEqual(
@"first
   second
      third".Replace("\r", string.Empty),
                result);
        }

        [Test]
        public void Dedent_WorksWithSingleLineInput()
        {
            var result = StringUtils.Dedent(@"A single line of input.");

            Assert.AreEqual("A single line of input.", result);
        }

        [Test]
        public void Dedent_WorksWithSingleLineAndClosingQuotationMarkOnNewLine()
        {
            var result = StringUtils.Dedent(@"
                A single line of input.
            ");

            Assert.AreEqual("A single line of input.", result);
        }

        [Test]
        public void Dedent_WorksWithSingleLineAndInlineClosingQuotationMark()
        {
            var result = StringUtils.Dedent(@"
                A single line of input.");

            Assert.AreEqual("A single line of input.", result);
        }

        [Test]
        public void Dedent_DoesntStripExplicitNewlines()
        {
            var result = StringUtils.Dedent(@"
                <p>Hello world!</p>\n
            ");

            Assert.AreEqual(
@"<p>Hello world!</p>
".Replace("\r", string.Empty),
                result);
        }

        [Test]
        public void Dedent_DoesntStripExplicitNewLinesWithMindent()
        {
            var result = StringUtils.Dedent(@"
                <p>
                  Hello world!
                </p>\n
            ");

            Assert.AreEqual(
@"<p>
  Hello world!
</p>
".Replace("\r", string.Empty),
                result);
        }
    }
}
