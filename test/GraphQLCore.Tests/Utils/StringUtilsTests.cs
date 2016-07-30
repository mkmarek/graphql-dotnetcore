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
    }
}
