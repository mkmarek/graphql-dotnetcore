namespace GraphQLCore.Tests.Exceptions
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class GraphQLExceptionTests
    {
        private Parser parser = new Parser(new Lexer());

        [Test]
        public void GraphQLException_IsAClassAndASubclassOfException()
        {
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(typeof(GraphQLException)));
        }

        [Test]
        public void GraphQLException_HasAMessageAndStackTrace()
        {
            try
            {
                throw new GraphQLException("msg");
            }
            catch (GraphQLException e)
            {
                Assert.AreEqual("msg", e.Message);
                Assert.IsInstanceOf<string>(e.StackTrace);
            }
        }

        [Test]
        public void GraphQLException_ConvertsNodesToPositionsAndLocations()
        {
            var source = new Source(
@"{
    field
}");
            var fieldNode = this.GetFieldNode(source);

            var e = new GraphQLException("msg", new[] { fieldNode });

            Assert.AreEqual(new[] { fieldNode }, e.Nodes);
            Assert.AreEqual(source, e.ASTSource);
            Assert.AreEqual(new[] { 6 }, e.Positions);
            Assert.AreEqual(new[] { new Location() { Line = 2, Column = 5 } }, e.Locations);
        }

        [Test]
        public void GraphQLException_ConvertsNodeWithZeroStartLocationToPositionsAndLocations()
        {
            var source = new Source(
@"{
    field
}");
            var operationNode = this.GetOperationDefinitionNode(source);
            var e = new GraphQLException("msg", new[] { operationNode });

            Assert.AreEqual(new[] { operationNode }, e.Nodes);
            Assert.AreEqual(source, e.ASTSource);
            Assert.AreEqual(new[] { 0 }, e.Positions);
            Assert.AreEqual(new[] { new Location() { Line = 1, Column = 1 } }, e.Locations);
        }

        [Test]
        public void GraphQLException_ConvertsSourceAndPositionsToLocations()
        {
            var source = new Source(
@"{
    field
}");
            var e = new GraphQLException("msg", null, source, new[] { 10 });

            Assert.AreEqual(null, e.Nodes);
            Assert.AreEqual(source, e.ASTSource);
            Assert.AreEqual(new[] { 10 }, e.Positions);
            Assert.AreEqual(new[] { new Location() { Line = 2, Column = 9 } }, e.Locations);
        }

        [Test]
        public void GraphQLException_SerializesToIncludeMessage()
        {
            var e = new GraphQLException("msg");

            Assert.AreEqual("{\"message\":\"msg\"}", e.ToString());
        }

        [Test]
        public void GraphQLException_SerializesToIncludeMessageAndLocations()
        {
            var node = this.GetFieldNode(new Source("{ field }"));

            var e = new GraphQLException("msg", new[] { node });

            Assert.AreEqual("{\"message\":\"msg\",\"locations\":[{\"line\":1,\"column\":3}]}",
                e.ToString());
        }

        [Test]
        public void GraphQLException_SerializesToIncludePath()
        {
            var e = new GraphQLException("msg", null, null, null, new object[] { "path", 3, "to", "field" });

            Assert.AreEqual(new object[] { "path", 3, "to", "field" }, e.Path);
            Assert.AreEqual("{\"message\":\"msg\",\"path\":[\"path\",3,\"to\",\"field\"]}", e.ToString());
        }

        private GraphQLOperationDefinition GetOperationDefinitionNode(ISource source)
        {
            return (GraphQLOperationDefinition)this.parser.Parse(source).Definitions.Single();
        }

        private GraphQLFieldSelection GetFieldNode(ISource source)
        {
            var operationDefinition = this.GetOperationDefinitionNode(source);

            return (GraphQLFieldSelection)operationDefinition.SelectionSet.Selections.Single();
        }   
    }

    public class Location
    {
        public int Column { get; set; }
        public int Line { get; set; }

        public override bool Equals(object obj)
        {
            var location = obj as GraphQLCore.Language.Location;
            if (location != null)
                return this.Column == location.Column && this.Line == location.Line;
            
            return base.Equals(obj);
        }
    }
}
