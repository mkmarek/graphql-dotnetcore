namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class FragmentsOnCompositeTypesTests : ValidationTestBase
    {
        [Test]
        public void FragmentOnObjectType_ReportsNoError()
        {
            var errors = Validate(@"
            fragment validFragment on SimpleObjectType {
                booleanField
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FragmentOnInterfaceType_ReportsNoError()
        {
            var errors = Validate(@"
            fragment validFragment on SimpleInterfaceType {
                booleanField
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InlineFragmentOnObjectType_ReportsNoError()
        {
            var errors = Validate(@"
            fragment validFragment on SimpleInterfaceType {
                ... on SimpleObjectType {
                    notInterfaceField
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InlineFragmentWithoutType_ReportsNoError()
        {
            var errors = Validate(@"
            fragment validFragment on SimpleInterfaceType {
                ... {
                    booleanField
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void InlineFragmentWithoutTypeOnRootType_ReportsNoError()
        {
            var errors = this.Validate(@"
            query foo { 
                ... {
                    __typename
                }
            }");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FragmentOnUnionType_ReportsNoError()
        {
            var errors = Validate(@"
            fragment validFragment on SimpleSampleUnionType {
                __typename
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FragmentOnScalarType_ReportsSingleError()
        {
            var errors = Validate(@"
            fragment scalarFragment on Boolean {
                bad
            }
            ");

            Assert.AreEqual("Fragment \"scalarFragment\" cannot condition on non composite type \"Boolean\".", errors.Single().Message);
        }

        [Test]
        public void FragmentOnEnumType_ReportsSingleError()
        {
            var errors = Validate(@"
            fragment scalarFragment on FurColor {
                bad
            }
            ");

            Assert.AreEqual("Fragment \"scalarFragment\" cannot condition on non composite type \"FurColor\".", errors.Single().Message);
        }

        [Test]
        public void FragmentOnInputObjectType_ReportsSingleError()
        {
            var errors = Validate(@"
            fragment inputFragment on ComplicatedInputObjectType {
                intField
            }
            ");

            Assert.AreEqual("Fragment \"inputFragment\" cannot condition on non composite type \"ComplicatedInputObjectType\".", errors.Single().Message);
        }

        [Test]
        public void InlineFragmentOnScalarType_ReportsSingleError()
        {
            var errors = Validate(@"
            fragment inputFragment on SimpleInterfaceType {
                ... on String {
                    __typename
                }
            }
            ");

            Assert.AreEqual("Fragment cannot condition on non composite type \"String\".", errors.Single().Message);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new FragmentsOnCompositeTypes()
                });
        }
    }
}
