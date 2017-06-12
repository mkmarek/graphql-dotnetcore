namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class NoUnusedFragmentsTests : ValidationTestBase
    {
        [Test]
        public void AllFragmentNamesAreUsed_DoesntReportAnyError()
        {
            var errors = Validate(@"
                {
                    human(id: 4) {
                      ...HumanFields1
                      ... on Human {
                        ...HumanFields2
                      }
                    }
                  }
                  fragment HumanFields1 on Human {
                    name
                    ...HumanFields3
                  }
                  fragment HumanFields2 on Human {
                    name
                  }
                  fragment HumanFields3 on Human {
                    name
                  }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void AllFragmentNamesAreUsedByMultipleOperations_DoesntReportAnyError()
        {
            var errors = Validate(@"
                query Foo {
                    human(id: 4) {
                      ...HumanFields1
                    }
                  }
                  query Bar {
                    human(id: 4) {
                      ...HumanFields2
                    }
                  }
                  fragment HumanFields1 on Human {
                    name
                    ...HumanFields3
                  }
                  fragment HumanFields2 on Human {
                    name
                  }
                  fragment HumanFields3 on Human {
                    name
                  }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ContainsUnknownFragments_ReportsTwoErrors()
        {
            var errors = Validate(@"
                query Foo {
                human(id: 4) {
                  ...HumanFields1
                }
              }
              query Bar {
                human(id: 4) {
                  ...HumanFields2
                }
              }
              fragment HumanFields1 on Human {
                name
                ...HumanFields3
              }
              fragment HumanFields2 on Human {
                name
              }
              fragment HumanFields3 on Human {
                name
              }
              fragment Unused1 on Human {
                name
              }
              fragment Unused2 on Human {
                name
              }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("Fragment \"Unused1\" is never used.", errors.ElementAt(0), 22, 15);
            ErrorAssert.AreEqual("Fragment \"Unused2\" is never used.", errors.ElementAt(1), 25, 15);
        }

        [Test]
        public void ContainsUnknownFragmentsWithCycle_ReportsTwoErrors()
        {
            var errors = Validate(@"
                query Foo {
                    human(id: 4) {
                      ...HumanFields1
                    }
                  }
                  query Bar {
                    human(id: 4) {
                      ...HumanFields2
                    }
                  }
                  fragment HumanFields1 on Human {
                    name
                    ...HumanFields3
                  }
                  fragment HumanFields2 on Human {
                    name
                  }
                  fragment HumanFields3 on Human {
                    name
                  }
                  fragment Unused1 on Human {
                    name
                    ...Unused2
                  }
                  fragment Unused2 on Human {
                    name
                    ...Unused1
                  }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("Fragment \"Unused1\" is never used.", errors.ElementAt(0), 22, 19);
            ErrorAssert.AreEqual("Fragment \"Unused2\" is never used.", errors.ElementAt(1), 26, 19);
        }

        [Test]
        public void ContainsUnknownAndUndefFragments_ReportsSingleError()
        {
            var errors = Validate(@"
               query Foo {
                human(id: 4) {
                  ...bar
                }
              }
              fragment foo on Human {
                name
              }
            ");

            ErrorAssert.AreEqual("Fragment \"foo\" is never used.", errors.Single(), 7, 15);
        }


        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new NoUnusedFragments()
                });
        }
    }
}
