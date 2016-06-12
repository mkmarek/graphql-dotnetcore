namespace GraphQL.Tests.Language
{
    using GraphQL.Language;
    using GraphQL.Language.AST;
    using NUnit.Framework;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Parse_FieldInput_HasCorrectEndLocationAttribute()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(9, document.Location.End);
        }

        [Test]
        public void Parse_FieldInput_HasCorrectStartLocationAttribute()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(0, document.Location.Start);
        }

        [Test]
        public void Parse_FieldInput_HasOneOperationDefinition()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(ASTNodeKind.OperationDefinition, document.Definitions.First().Kind);
        }

        [Test]
        public void Parse_FieldInput_NameIsNull()
        {
            var document = ParseGraphQLFieldSource();

            Assert.IsNull(GetSingleOperationDefinition(document).Name);
        }

        [Test]
        public void Parse_FieldInput_OperationIsQuery()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(OperationType.Query, GetSingleOperationDefinition(document).Operation);
        }

        [Test]
        public void Parse_FieldInput_ReturnsDocumentNode()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(ASTNodeKind.Document, document.Kind);
        }

        [Test]
        public void Parse_FieldInput_SelectionSetContainsSingleFieldSelection()
        {
            var document = ParseGraphQLFieldSource();

            Assert.AreEqual(ASTNodeKind.Field, GetSingleSelection(document).Kind);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_HasCorrectEndLocationAttribute()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(22, document.Location.End);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_HasCorrectStartLocationAttribute()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(0, document.Location.Start);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_HasOneOperationDefinition()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(ASTNodeKind.OperationDefinition, document.Definitions.First().Kind);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_NameIsNull()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual("Foo", GetSingleOperationDefinition(document).Name.Value);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_OperationIsQuery()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(OperationType.Mutation, GetSingleOperationDefinition(document).Operation);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_ReturnsDocumentNode()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(ASTNodeKind.Document, document.Kind);
        }

        [Test]
        public void Parse_FieldWithOperationTypeAndNameInput_SelectionSetContainsSingleFieldWithOperationTypeAndNameSelection()
        {
            var document = ParseGraphQLFieldWithOperationTypeAndNameSource();

            Assert.AreEqual(ASTNodeKind.Field, GetSingleSelection(document).Kind);
        }

        [Test]
        public void Parse_VariableInlineValues_DoesNotThrowError()
        {
            new Parser(new Lexer()).Parse(new Source("{ field(complex: { a: { b: [ $var ] } }) }"));
        }

        [Test]
        public void Parse_KitchenSink_DoesNotThrowError()
        {
            new Parser(new Lexer()).Parse(new Source(LoadKitchenSink()));
        }

        private static string LoadKitchenSink()
        {
            string dataFilePath = Directory.GetCurrentDirectory() + "\\data\\KitchenSink.graphql";
            return File.ReadAllText(dataFilePath);
        }

        private static GraphQLOperationDefinition GetSingleOperationDefinition(GraphQLDocument document)
        {
            return ((GraphQLOperationDefinition)document.Definitions.Single());
        }

        private static ASTNode GetSingleSelection(GraphQLDocument document)
        {
            return GetSingleOperationDefinition(document).SelectionSet.Selections.Single();
        }

        private static GraphQLDocument ParseGraphQLFieldSource()
        {
            return new Parser(new Lexer()).Parse(new Source("{ field }"));
        }

        private static GraphQLDocument ParseGraphQLFieldWithOperationTypeAndNameSource()
        {
            return new Parser(new Lexer()).Parse(new Source("mutation Foo { field }"));
        }
    }
}