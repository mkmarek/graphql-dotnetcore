namespace GraphQLCore.Tests.Validation
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language;
    using NUnit.Framework;
    using System.Linq;

    public static class ErrorAssert
    {
        public static void AreEqual(string message, GraphQLException actual, int line, int column)
        {
            Assert.AreEqual(message, actual.Message);

            var singleLocation = actual.Locations.Single();
            AssertLocation(line, column, singleLocation);
        }

        public static void StartsWith(string message, GraphQLException actual, int line, int column)
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
    }
}
