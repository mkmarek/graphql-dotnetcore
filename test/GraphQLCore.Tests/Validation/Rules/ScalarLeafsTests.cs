namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class ScalarLeafsTests : ValidationTestBase
    {
        [Test]
        public void ValidScalarSelection()
        {
            var errors = this.Validate(@"
                fragment scalarSelection on ComplicatedInterfaceType { intField }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ObjectTypeMissingSelection()
        {
            var errors = this.Validate(@"
                query directQueryOnObjectWithoutSubFields { complicatedArgs }");

            ErrorAssert.AreEqual(
                "Field \"complicatedArgs\" of type \"ComplicatedArgs\" must have a selection of subfields. " +
                "Did you mean \"complicatedArgs { ... }\"?",
                errors.Single(), 2, 61);
        }

        [Test]
        public void InterfaceTypeWithMissingSelection()
        {
            var errors = this.Validate(@"
                query directQueryOnObjectWithoutSubFields { interfaceObject }");

            ErrorAssert.AreEqual(
                "Field \"interfaceObject\" of type \"ComplicatedInterfaceType\" must have a selection of subfields. " +
                "Did you mean \"interfaceObject { ... }\"?",
                errors.Single(), 2, 61);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnBoolean()
        {
            var errors = this.Validate(@"
                query directQueryOnObjectWithoutSubFields { interfaceObject { booleanField { stuff } } }");

            ErrorAssert.AreEqual(
                "Field \"booleanField\" must not have a selection since " +
                "type \"Boolean\" has no subfields.",
                errors.Single(), 2, 92);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnEnum()
        {
            var errors = this.Validate(@"
                query directQueryOnObjectWithoutSubFields { interfaceObject { enumField { stuff } } }");

            ErrorAssert.AreEqual(
                "Field \"enumField\" must not have a selection since " +
                "type \"FurColor\" has no subfields.",
                errors.Single(), 2, 89);
        }

        [Test]
        public void ScalarSelectionNotAllowedOnListOfString()
        {
            var errors = this.Validate(@"
                query directQueryOnObjectWithoutSubFields { interfaceObject { stringListField { stuff } } }");

            ErrorAssert.AreEqual(
                "Field \"stringListField\" must not have a selection since " +
                "type \"[String]\" has no subfields.",
                errors.Single(), 2, 95);
        }

        [Test]
        public void IntrospectedObjectTypeMissingSelection()
        {
            var errors = this.Validate(@"
                query directIntrospectionQuerytWithoutSubFields { __schema }");

            ErrorAssert.AreEqual(
                "Field \"__schema\" of type \"__Schema\" must have a selection of subfields. " +
                "Did you mean \"__schema { ... }\"?",
                errors.Single(), 2, 67);
        }
    }
}