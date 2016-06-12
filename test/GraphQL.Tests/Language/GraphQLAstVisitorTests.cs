namespace GraphQL.Tests.Language
{
    using GraphQL.Language;
    using NUnit.Framework;
    using NSubstitute;
    using GraphQL.Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    [TestFixture]
    public class GraphQLAstVisitorTests
    {
        private GraphQLAstVisitor visitor;
        private Parser parser;
        private List<ASTNode> visitedDefinitions;
        private List<GraphQLSelectionSet> visitedSelectionSets;
        private List<GraphQLFieldSelection> visitedFieldSelections;
        private List<GraphQLName> visitedNames;
        private List<GraphQLArgument> visitedArguments;
        private List<GraphQLName> visitedAliases;
        private List<GraphQLFragmentSpread> visitedFragmentSpreads;
        private List<GraphQLNamedType> visitedFragmentTypeConditions;
        private List<GraphQLFragmentDefinition> visitedFragmentDefinitions;
        private List<GraphQLInlineFragment> visitedInlineFragments;
        private List<GraphQLDirective> visitedDirectives;
        private List<GraphQLVariable> visitedVariables;
        private List<GraphQLValue<int>> visitedIntValues;
        private List<GraphQLValue<float>> visitedFloatValues;
        private List<GraphQLValue<string>> visitedStringValues;
        private List<GraphQLValue<string>> visitedEnumValues;

        public List<GraphQLValue<bool>> visitedBooleanValues { get; private set; }

        [Test]
        public void Visit_OneDefinition_CallsVisitDefinitionOnce()
        {
            this.visitor.Visit(this.Parse("{ a }"));

            Assert.AreEqual(1, visitedDefinitions.Count);
        }

        [Test]
        public void Visit_OneDefinition_ProvidesCorrectDefinitionAsParameter()
        {
            var ast = this.Parse("{ a }");
            this.visitor.Visit(ast);

            Assert.AreEqual(ast.Definitions.Single(), visitedDefinitions.Single());
        }

        [Test]
        public void Visit_OneDefinition_VisitsOneSelectionSet()
        {
            this.visitor.Visit(this.Parse("{ a, b }"));

            Assert.AreEqual(1, this.visitedSelectionSets.Count);
        }

        [Test]
        public void Visit_TwoFieldSelections_VisitsFieldSelectionTwice()
        {
            this.visitor.Visit(this.Parse("{ a, b }"));

            Assert.AreEqual(2, this.visitedFieldSelections.Count);
        }


        [Test]
        public void Visit_TwoFieldSelections_VisitsTwoFieldNames()
        {
            this.visitor.Visit(this.Parse("{ a, b }"));

            Assert.AreEqual(2, this.visitedNames.Count);
        }

        [Test]
        public void Visit_TwoFieldSelections_VisitsTwoFieldNamesAndDefinitionName()
        {
            this.visitor.Visit(this.Parse("query foo { a, b }"));

            Assert.AreEqual(3, this.visitedNames.Count);
        }

        [Test]
        public void Visit_TwoFieldSelectionsWithOneNested_VisitsFiveFieldSelections()
        {
            this.visitor.Visit(this.Parse("{a, nested { x,  y }, b}"));

            Assert.AreEqual(5, this.visitedFieldSelections.Count);
        }

        [Test]
        public void Visit_TwoFieldSelectionsWithOneNested_VisitsFiveNames()
        {
            this.visitor.Visit(this.Parse("{a, nested { x,  y }, b}"));

            Assert.AreEqual(5, this.visitedNames.Count);
        }

        [Test]
        public void Visit_TwoDefinitions_CallsVisitDefinitionTwice()
        {
            this.visitor.Visit(this.Parse("{ a }\n{ b }"));

            Assert.AreEqual(2, visitedDefinitions.Count);
        }

        [Test]
        public void Visit_OneDefinitionWithOneArgument_VisitsOneArgument()
        {
            this.visitor.Visit(this.Parse("{ foo(id : 1) { name } }"));

            Assert.AreEqual(1, this.visitedArguments.Count);
        }

        [Test]
        public void Visit_OneDefinitionWithOneNestedArgument_VisitsOneArgument()
        {
            this.visitor.Visit(this.Parse("{ foo{ names(size: 10) } }"));

            Assert.AreEqual(1, this.visitedArguments.Count);
        }

        [Test]
        public void Visit_OneDefinitionWithOneAliasedField_VisitsOneAlias()
        {
            this.visitor.Visit(this.Parse("{ foo, foo : bar }"));

            Assert.AreEqual(1, this.visitedAliases.Count);
        }

        [Test]
        public void Visit_DefinitionWithSingleFragmentSpread_VisitsFragmentSpreadOneTime()
        {
            this.visitor.Visit(this.Parse("{ foo { ...fragment } }"));

            Assert.AreEqual(1, this.visitedFragmentSpreads.Count);
        }

        [Test]
        public void Visit_DefinitionWithSingleFragmentSpread_VisitsNameOfPropertyAndFragmentSpread()
        {
            this.visitor.Visit(this.Parse("{ foo { ...fragment } }"));

            Assert.AreEqual(2, this.visitedNames.Count);
        }

        [Test]
        public void Visit_FragmentWithTypeCondition_VisitsFragmentDefinitionOnce()
        {
            this.visitor.Visit(this.Parse("fragment testFragment on Stuff { field }"));

            Assert.AreEqual(1, this.visitedFragmentDefinitions.Count);
        }

        [Test]
        public void Visit_FragmentWithTypeCondition_VisitsTypeConditionOnce()
        {
            this.visitor.Visit(this.Parse("fragment testFragment on Stuff { field }"));

            Assert.AreEqual(1, this.visitedFragmentTypeConditions.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithTypeCondition_VisitsInlineFragmentOnce()
        {
            this.visitor.Visit(this.Parse("{ ... on Stuff { field } }"));

            Assert.AreEqual(1, this.visitedInlineFragments.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithTypeCondition_VisitsTypeConditionOnce()
        {
            this.visitor.Visit(this.Parse("{ ... on Stuff { field } }"));

            Assert.AreEqual(1, this.visitedFragmentTypeConditions.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithDirectiveAndArgument_VisitsDirectiveOnce()
        {
            this.visitor.Visit(this.Parse("{ ... @include(if : $stuff) { field } }"));

            Assert.AreEqual(1, this.visitedDirectives.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithDirectiveAndArgument_VisitsArgumentsOnce()
        {
            this.visitor.Visit(this.Parse("{ ... @include(if : $stuff) { field } }"));

            Assert.AreEqual(1, this.visitedArguments.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithDirectiveAndArgument_VisitsNameThreeTimes()
        {
            this.visitor.Visit(this.Parse("{ ... @include(if : $stuff) { field } }"));

            Assert.AreEqual(4, this.visitedNames.Count);
        }

        [Test]
        public void Visit_DirectiveWithVariable_VisitsVariableOnce()
        {
            this.visitor.Visit(this.Parse("{ ... @include(if : $stuff) { field } }"));

            Assert.AreEqual(1, this.visitedVariables.Count);
        }

        [Test]
        public void Visit_InlineFragmentWithOneField_VisitsOneField()
        {
            this.visitor.Visit(this.Parse("{ ... @include(if : $stuff) { field } }"));

            Assert.AreEqual(1, this.visitedFieldSelections.Count);
        }

        [Test]
        public void Visit_IntValueArgument_VisitsOneIntValue()
        {
            this.visitor.Visit(this.Parse("{ stuff(id : 1) }"));

            Assert.AreEqual(1, this.visitedIntValues.Count);
        }

        [Test]
        public void Visit_FloatValueArgument_VisitsOneFloatValue()
        {
            this.visitor.Visit(this.Parse("{ stuff(id : 1.2) }"));

            Assert.AreEqual(1, this.visitedFloatValues.Count);
        }

        [Test]
        public void Visit_StringValueArgument_VisitsOneStringValue()
        {
            this.visitor.Visit(this.Parse("{ stuff(id : \"abc\") }"));

            Assert.AreEqual(1, this.visitedStringValues.Count);
        }

        [Test]
        public void Visit_BooleanValueArgument_VisitsOneBooleanValue()
        {
            this.visitor.Visit(this.Parse("{ stuff(id : true) }"));

            Assert.AreEqual(1, this.visitedBooleanValues.Count);
        }

        [Test]
        public void Visit_EnumValueArgument_VisitsOneEnumValue()
        {
            this.visitor.Visit(this.Parse("{ stuff(id : TEST_ENUM) }"));

            Assert.AreEqual(1, this.visitedEnumValues.Count);
        }

        private GraphQLDocument Parse(string expression)
        {
            return this.parser.Parse(new Source(expression));
        }

        [SetUp]
        public void SetUp()
        {
            this.parser = new Parser(new Lexer());
            this.visitor = Substitute.ForPartsOf<GraphQLAstVisitor>();

            this.visitedDefinitions = MockVisitMethod<ASTNode>((visitor) => visitor.VisitOperationDefinition(null));
            this.visitedSelectionSets = MockVisitMethod<GraphQLSelectionSet>((visitor) => visitor.VisitSelectionSet(null));
            this.visitedFieldSelections = MockVisitMethod<GraphQLFieldSelection>((visitor) => visitor.VisitFieldSelection(null));
            this.visitedNames = MockVisitMethod<GraphQLName>((visitor) => visitor.VisitName(null));
            this.visitedArguments = MockVisitMethod<GraphQLArgument>((visitor) => visitor.VisitArgument(null));
            this.visitedAliases = MockVisitMethod<GraphQLName>((visitor) => visitor.VisitAlias(null));
            this.visitedFragmentSpreads = MockVisitMethod<GraphQLFragmentSpread>((visitor) => visitor.VisitFragmentSpread(null));
            this.visitedFragmentDefinitions = MockVisitMethod<GraphQLFragmentDefinition>((visitor) => visitor.VisitFragmentDefinition(null));
            this.visitedFragmentTypeConditions = MockVisitMethod<GraphQLNamedType>((visitor) => visitor.VisitNamedType(null));
            this.visitedInlineFragments = MockVisitMethod<GraphQLInlineFragment>((visitor) => visitor.VisitInlineFragment(null));
            this.visitedDirectives = MockVisitMethod<GraphQLDirective>((visitor) => visitor.VisitDirective(null));
            this.visitedVariables = MockVisitMethod<GraphQLVariable>((visitor) => visitor.VisitVariable(null));
            this.visitedIntValues = MockVisitMethod<GraphQLValue<int>>((visitor) => visitor.VisitIntValue(null));
            this.visitedFloatValues = MockVisitMethod<GraphQLValue<float>>((visitor) => visitor.VisitFloatValue(null));
            this.visitedStringValues = MockVisitMethod<GraphQLValue<string>>((visitor) => visitor.VisitStringValue(null));
            this.visitedBooleanValues = MockVisitMethod<GraphQLValue<bool>>((visitor) => visitor.VisitBooleanValue(null));
            this.visitedEnumValues = MockVisitMethod<GraphQLValue<string>>((visitor) => visitor.VisitEnumValue(null));
        }

        private List<TEntity> MockVisitMethod<TEntity>(Action<GraphQLAstVisitor> visitorMethod)
        {
            var collection = new List<TEntity>();
            this.visitor.WhenForAnyArgs(visitorMethod)
                .Do(e => { collection.Add(e.Arg<TEntity>()); });

            return collection;
        }
    }
}
