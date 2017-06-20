namespace GraphQLCore.Tests.Validation
{
    using System.Collections;
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language;
    using NUnit.Framework;
    using System.Linq;

    public static class ErrorAssert
    {
        public static void AreEqual(string message, GraphQLException actual, int line, int column, IEnumerable path = null)
        {
            Assert.AreEqual(message, actual.Message);

            var singleLocation = actual.Locations.Single();
            AssertLocation(line, column, singleLocation);

            if (path != null)
                AssertPath(path, actual.Path);
        }

        public static void StartsWith(string message, GraphQLException actual, int line, int column, IEnumerable path = null)
        {
            Assert.IsTrue(actual.Message.StartsWith(message));

            var singleLocation = actual.Locations.Single();
            AssertLocation(line, column, singleLocation);
        }

        public static void AreEqual(string message, GraphQLException actual, params int[][] locations)
        {
            Assert.AreEqual(message, actual.Message);

            if (locations != null)
                AssertLocations(locations, actual.Locations);
        }

        private static void AssertLocation(int line, int column, Location actual)
        {
            Assert.AreEqual(line, actual.Line);
            Assert.AreEqual(column, actual.Column);
        }

        private static void AssertLocations(int[][] locations, Location[] actual)
        {
            Assert.AreEqual(locations, actual.Select(e => new[] { e.Line, e.Column }).ToArray());
        }

        private static void AssertPath(IEnumerable expected, IEnumerable actual)
        {
            foreach (var element in actual)
                Assert.IsTrue(element is string || element is int);

            Assert.AreEqual(expected, actual);
        }
    }
}
