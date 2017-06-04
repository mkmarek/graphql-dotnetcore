namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class PossibleFragmentSpreadsTests : ValidationTestBase
    {
        [Test]
        public void OnTheSameObject_Passes()
        {
            var errors = this.Validate(@"
                fragment objectWithinObject on ComplicatedObjectType { ...complicatedObjectFragment }
                fragment complicatedObjectFragment on ComplicatedObjectType { intField }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void OnTheSameObjectWithInlineFragment_Passes()
        {
            var errors = this.Validate(@"
                fragment objectWithinObject on ComplicatedObjectType { ... on ComplicatedObjectType { intField } }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ObjectIntoAnImplementedInterface_Passes()
        {
            var errors = this.Validate(@"
                fragment objectWithinInterface on ComplicatedInterfaceType { ... on ComplicatedObjectType { intField } }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InterfaceIntoImplementedObject_Pases()
        {
            var errors = this.Validate(@"
                fragment objectWithinInterface on ComplicatedObjectType { ... on ComplicatedInterfaceType { intField } }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InterfaceIntoOverlappingInterface_Pases()
        {
            var errors = this.Validate(@"
                fragment interfaceWithinInterface on ComplicatedParentInterfaceType { ...interfaceFrag }
                fragment interfaceFrag on ComplicatedInterfaceType { intField }
                ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ArrayOfInterfacesIntoTypeInline_Pases()
        {
            var errors = this.Validate(@"
                query {
                    interfaceObjectArray {
                        ... on ComplicatedObjectType {
                            BooleanField
                        }
                    }
                }
                ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ArrayOfInterfacesIntoTypeFragment_Pases()
        {
            var errors = this.Validate(@"
                query {
                    interfaceObjectArray {
                        ...frag
                    }
                }

                fragment frag on ComplicatedObjectType {
                   BooleanField
                }
                ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InterfaceIntoOverlappingInterfaceInlineFragment_Pases()
        {
            var errors = this.Validate(@"
                fragment interfaceWithinInterface on ComplicatedParentInterfaceType { ... on ComplicatedInterfaceType { intField } }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DifferentObjectIntoObject_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidObjectWithinObject on ComplicatedObjectType { ...otherObject }
                fragment otherObject on SimpleObjectType { booleanField }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment otherObject cannot be spread here as objects of " +
                "type \"ComplicatedObjectType\" can never be of type \"SimpleObjectType\".",
                error.Message);
        }

        [Test]
        public void DifferentObjectIntoObjectInline_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidObjectWithinObject on ComplicatedObjectType { 
                    ... on SimpleObjectType { booleanField } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment cannot be spread here as objects of " +
                "type \"ComplicatedObjectType\" can never be of type \"SimpleObjectType\".",
                error.Message);
        }

        [Test]
        public void ObjectIntoNotImplementingInterface_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidObjectWithinInterface on ComplicatedInterfaceType { 
                    ... on SimpleObjectType { booleanField } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment cannot be spread here as objects of " +
                "type \"ComplicatedInterfaceType\" can never be of type \"SimpleObjectType\".",
                error.Message);
        }

        [Test]
        public void InterfaceIntoNonImplementingObject_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidInterfaceWithinObject on SimpleObjectType { 
                    ... on ComplicatedInterfaceType { booleanField } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment cannot be spread here as objects of " +
                "type \"SimpleObjectType\" can never be of type \"ComplicatedInterfaceType\".",
                error.Message);
        }

        [Test]
        public void InterfaceIntoNonOverlappingInterface_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidInterfaceWithinInterface on SimpleInterfaceType { ...interfaceFrag }
                fragment interfaceFrag on ComplicatedInterfaceType { booleanField }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment interfaceFrag cannot be spread here as objects of " +
                "type \"SimpleInterfaceType\" can never be of type \"ComplicatedInterfaceType\".",
                error.Message);
        }

        [Test]
        public void InterfaceIntoNonOverlappingInterfaceInlineFragment_Fails()
        {
            var errors = this.Validate(@"
                fragment invalidInterfaceWithinInterface on SimpleInterfaceType { 
                    ... on ComplicatedInterfaceType { booleanField } }");

            var error = errors.Single();

            Assert.AreEqual(
                "Fragment cannot be spread here as objects of " +
                "type \"SimpleInterfaceType\" can never be of type \"ComplicatedInterfaceType\".",
                error.Message);
        }
    }
}