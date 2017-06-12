namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class KnownFragmentNamesTests : ValidationTestBase
    {
        [Test]
        public void KnownFragmentNames_AreValid()
        {
            var errors = Validate(@"
                {
                human(id: 4) {
                    ...HumanFields1
                    ... on Human {
                        ...HumanFields2
                    }
                    ... {
                        name
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
        public void UnknownFragmentNames_AreInvalid()
        {
            var errors = Validate(@"
                {
                    human(id: 4) {
                    ...UnknownFragment1
                    ... on Human {
                        ...UnknownFragment2
                    }
                    }
                }
                fragment HumanFields on Human {
                    name
                    ...UnknownFragment3
                }
            ");

            ErrorAssert.AreEqual("Unknown fragment \"UnknownFragment1\".", errors.ElementAt(0), 4, 24);
            ErrorAssert.AreEqual("Unknown fragment \"UnknownFragment2\".", errors.ElementAt(1), 6, 28);
            ErrorAssert.AreEqual("Unknown fragment \"UnknownFragment3\".", errors.ElementAt(2), 12, 24);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new KnownFragmentNames()
                });
        }
    }
}
