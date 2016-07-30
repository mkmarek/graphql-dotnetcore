namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class ScalarLeafsTests : ValidationTestBase
    {
        [Test]
        public void ValidScalarSelection()
        {
            var errors = this.Validate("fragment scalarSelection on ComplicatedInterfaceType { intField }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ObjectTypeMissingSelection()
        {
            var errors = this.Validate("query directQueryOnObjectWithoutSubFields { complicatedArgs }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"complicatedArgs\" of type \"ComplicatedArgs\" must have a selection of subfields. " +
                "Did you mean \"complicatedArgs { ... }\"?",
                error.Message);
        }

        [Test]
        public void InterfaceTypeWithMissingSelection()
        {
            var errors = this.Validate("query directQueryOnObjectWithoutSubFields { interfaceObject }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"interfaceObject\" of type \"ComplicatedInterfaceType\" must have a selection of subfields. " +
                "Did you mean \"interfaceObject { ... }\"?",
                error.Message);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnBoolean()
        {
            var errors = this.Validate(
                "query directQueryOnObjectWithoutSubFields { interfaceObject { booleanField { stuff } } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"booleanField\" must not have a selection since " +
                "type \"Boolean\" has no subfields.",
                error.Message);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnEnum()
        {
            var errors = this.Validate(
                "query directQueryOnObjectWithoutSubFields { interfaceObject { enumField { stuff } } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"enumField\" must not have a selection since " +
                "type \"FurColor\" has no subfields.",
                error.Message);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnListOfString()
        {
            var errors = this.Validate(
                "query directQueryOnObjectWithoutSubFields { interfaceObject { stringListField { stuff } } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"stringListField\" must not have a selection since " +
                "type \"String\" has no subfields.",
                error.Message);
        }

        [Test]
        public void IntrospectedObjectTypeMissingSelection()
        {
            var errors = this.Validate("query directIntrospectionQuerytWithoutSubFields { __schema }");

            var error = errors.Single();

            Assert.AreEqual(
                "Field \"__schema\" of type \"__Schema\" must have a selection of subfields. " +
                "Did you mean \"__schema { ... }\"?",
                error.Message);
        }
    }
}