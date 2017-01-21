namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class NoFragmentCyclesTests : ValidationTestBase
    {
        [Test]
        public void NoCircularFragmentSpreads_DoesntReportAnyError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB }
      			fragment fragB on Dog { name }
            ");

            Assert.IsEmpty(errors);
        }

		[Test]
        public void SpreadingTwiceIsNotCircular_DoesntReportAnyError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB, ...fragB }
      			fragment fragB on Dog { name }
            ");

            Assert.IsEmpty(errors);
        }

		[Test]
        public void SpreadingTwiceIndirectlyIsNotCircular_DoesntReportAnyError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB, ...fragC }
				fragment fragB on Dog { ...fragC }
				fragment fragC on Dog { name }
            ");

            Assert.IsEmpty(errors);
        }

		[Test]
        public void DoubleSpreadWithinAbstractTypes_DoesntReportAnyError()
        {
            var errors = Validate(@"
                fragment nameFragment on Pet {
					... on Dog { name }
					... on Cat { name }
				}
				fragment spreadsInAnon on Pet {
					... on Dog { ...nameFragment }
					... on Cat { ...nameFragment }
				}
            ");

            Assert.IsEmpty(errors);
        }

		[Test]
        public void DoesNotFalsePositiveOnUnknownFragments()
        {
            var errors = Validate(@"
                fragment nameFragment on Pet {
					...UnknownFragment
				}
            ");

            Assert.IsEmpty(errors);
        }

		[Test]
        public void SpreadingRecursivelyWithinField_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Human { relatives { ...fragA } }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragA\" within itself.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfDirectly_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragA }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragA\" within itself.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfDirectlyWithinInlineFragment_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Pet {
                    ... on Dog {
                    ...fragA
                    }
                }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragA\" within itself.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfIndirectly_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB }
                fragment fragB on Dog { ...fragA }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragA\" within itself via fragB.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfIndirectlyInReverseOrder_ReportsError()
        {
            var errors = Validate(@"
                fragment fragB on Dog { ...fragA }
                fragment fragA on Dog { ...fragB }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragB\" within itself via fragA.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfIndirectlyWithinInlineFragment_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Pet {
                    ... on Dog {
                    ...fragB
                    }
                }
                fragment fragB on Pet {
                    ... on Dog {
                    ...fragA
                    }
                }
            ");

            Assert.AreEqual("Cannot spread fragment \"fragA\" within itself via fragB.", errors.Single().Message);
        }

        [Test]
        public void SpreadingItselfDeeply_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB }
                fragment fragB on Dog { ...fragC }
                fragment fragC on Dog { ...fragO }
                fragment fragX on Dog { ...fragY }
                fragment fragY on Dog { ...fragZ }
                fragment fragZ on Dog { ...fragO }
                fragment fragO on Dog { ...fragP }
                fragment fragP on Dog { ...fragA, ...fragX }
            ");

            Assert.AreEqual(
                "Cannot spread fragment \"fragA\" within itself via fragB, fragC, fragO, fragP.", 
                errors.First().Message);

            Assert.AreEqual(
                "Cannot spread fragment \"fragO\" within itself via fragP, fragX, fragY, fragZ.", 
                errors.Last().Message);
        }

        [Test]
        public void SpreadingItselfDeeplyWithTwoPaths_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB, ...fragC }
                fragment fragB on Dog { ...fragA }
                fragment fragC on Dog { ...fragA }
            ");

            Assert.AreEqual(
                "Cannot spread fragment \"fragA\" within itself via fragB.", 
                errors.First().Message);

            Assert.AreEqual(
                "Cannot spread fragment \"fragA\" within itself via fragC.", 
                errors.Last().Message);
        }

        [Test]
        public void SpreadingItselfDeeplyWithTwoPathsAltTraverseOrder_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragC }
                fragment fragB on Dog { ...fragC }
                fragment fragC on Dog { ...fragA, ...fragB }
            ");

            Assert.AreEqual(
                "Cannot spread fragment \"fragA\" within itself via fragC.", 
                errors.First().Message);

            Assert.AreEqual(
                "Cannot spread fragment \"fragC\" within itself via fragB.", 
                errors.Last().Message);
        }

        [Test]
        public void SpreadingItselfDeeplyAndImmediately_ReportsError()
        {
            var errors = Validate(@"
                fragment fragA on Dog { ...fragB }
                fragment fragB on Dog { ...fragB, ...fragC }
                fragment fragC on Dog { ...fragA, ...fragB }
            ");

            Assert.AreEqual(
                "Cannot spread fragment \"fragB\" within itself.", 
                errors.ElementAt(0).Message);

            Assert.AreEqual(
                "Cannot spread fragment \"fragA\" within itself via fragB, fragC.", 
                errors.ElementAt(1).Message);

            Assert.AreEqual(
                "Cannot spread fragment \"fragB\" within itself via fragC.", 
                errors.ElementAt(2).Message);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new NoFragmentCycles()
                });
        }
    }
}
