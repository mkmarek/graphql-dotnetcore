namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class UniqueDirectivesPerLocationTests : ValidationTestBase
    {
        [Test]
        public void NoDirectivePasses()
        {
            var errors = this.Validate(@"
                fragment Test on Type {
                    field
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void UniqueDirectivesInDifferentLocationsPasses()
        {
            var errors = this.Validate(@"
                fragment Test on Type @directiveA {
                    field @directiveB
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void UniqueDirectivesInSameLocationsPasses()
        {
            var errors = this.Validate(@"
                fragment Test on Type @directiveA @directiveB {
                    field @directiveA @directiveB
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameDirectivesInDifferentLocationsPasses()
        {
            var errors = this.Validate(@"
                fragment Test on Type @directiveA {
                    field @directiveA
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameDirectivesInSimilarLocations()
        {
            var errors = this.Validate(@"
                 fragment Test on Type {
                    field @directive
                    field @directive
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DuplicateDirectivesInOneLocationFails()
        {
            var errors = this.Validate(@"
                fragment Test on Type {
                    field @directive @directive
                }
            ");

            Assert.AreEqual("The directive directive can only be used once at this location.",
                errors.Single().Message);
        }

        [Test]
        public void ManyDuplicateDirectivesInOneLocationFails()
        {
            var errors = this.Validate(@"
                fragment Test on Type {
                    field @directive @directive @directive
                }
            ");

            Assert.AreEqual("The directive directive can only be used once at this location.",
                errors.ElementAt(0).Message);
            Assert.AreEqual("The directive directive can only be used once at this location.",
                errors.ElementAt(1).Message);
        }

        [Test]
        public void DifferentDuplicateDirectivesInOneLocationFails()
        {
            var errors = this.Validate(@"
                fragment Test on Type {
                    field @directiveA @directiveB @directiveA @directiveB
                }
            ");

            Assert.AreEqual("The directive directiveA can only be used once at this location.",
                errors.ElementAt(0).Message);
            Assert.AreEqual("The directive directiveB can only be used once at this location.",
                errors.ElementAt(1).Message);
        }

        [Test]
        public void DuplicateDirectivesInManyLocationsFail()
        {
            var errors = this.Validate(@"
                fragment Test on Type @directive @directive {
                    field @directive @directive
                }
            ");

            Assert.AreEqual("The directive directive can only be used once at this location.",
                errors.ElementAt(0).Message);
            Assert.AreEqual("The directive directive can only be used once at this location.",
                errors.ElementAt(1).Message);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new UniqueDirectivesPerLocation()
                });
        }
    }
}