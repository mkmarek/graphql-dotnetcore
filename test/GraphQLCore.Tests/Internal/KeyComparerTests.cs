namespace GraphQLCore.Tests.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQLCore.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class KeyComparerTests
    {
        [Test]
        public void KeyComparer_ComparesCorrectly()
        {
            var unsorted = new List<int[]>()
            {
                null,
                new[] { 0 },
                new[] { 0, 0, 0 },
                new[] { 0, 1, 0 },
                new[] { 0, 0, 1 },
                new[] { 0, 0, 0, 0 },
                new[] { 0, 2 },
                new[] { 1, 0, 0 },
                new[] { 10 },
                new[] { 1, 2, 3 }
            };

            unsorted.Sort(new KeyComparer());

            Assert.AreEqual(new List<int[]>()
            {
                null,
                new[] { 0 },
                new[] { 0, 0, 0 },
                new[] { 0, 0, 0, 0 },
                new[] { 0, 0, 1 },
                new[] { 0, 1, 0 },
                new[] { 0, 2 },
                new[] { 1, 0, 0 },
                new[] { 1, 2, 3 },
                new[] { 10 }
            }, unsorted);
        }
    }
}
