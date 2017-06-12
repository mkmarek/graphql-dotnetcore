namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class KnownArgumentNamesTests : ValidationTestBase
    {
        [Test]
        public void SingleKnownArgument_ReportsNoError()
        {
            var errors = Validate(@"
            fragment argOnRequiredArg on ComplicatedArgs {
                enumArgField(enumArg: BLACK)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleKnownArguments_ReportsNoError()
        {
            var errors = Validate(@"
            fragment multipleArgs on ComplicatedArgs {
                nonNullIntMultipleArgsField(arg1: 1, arg2: 2)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ArgumentOnUnknownField_ReportsNoError()
        {
            var errors = Validate(@"
            fragment argOnUnknownField on ComplicatedArgs {
                unknownField(unknownArg: UNKNOWN)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleKnownArgumentsNotInOrder_ReportsNoError()
        {
            var errors = Validate(@"
            fragment multipleArgsReverseOrder on ComplicatedArgs {
                nonNullIntMultipleArgsField(arg2: 2, arg1: 1)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void MissingOptionalArgument_ReportsNoError()
        {
            var errors = Validate(@"
            fragment noArgOnOptionalArg on ComplicatedArgs {
                stringArgField
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DeeplyNestedKnownArgument_ReportsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: BLACK)
                }
                args : complicatedArgs {
                    nested {
                        ... on ComplicatedArgs {
                            enumArgField(enumArg: BLACK)
                        }
                    }
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DirectiveKnownArgument_ReportsNoError()
        {
            var errors = Validate(@"
            {
                foo @skip(if: true)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DirectiveUnknownArgument_ReportsSingleError()
        {
            var errors = Validate(@"
            {
                foo @skip(gif: true)
            }
            ");

            ErrorAssert.AreEqual("Unknown argument \"gif\" on directive \"skip\". Did you mean \"if\"?",
                errors.Single(), 3, 27);
        }

        [Test]
        public void InvalidArgumentName_ReportsSingleError()
        {
            var errors = Validate(@"
            fragment invalidArgName on ComplicatedArgs {
                enumArgField(unknown: BLACK)
            }
            ");

            ErrorAssert.AreEqual("Unknown argument \"unknown\" on field \"enumArgField\" of type \"ComplicatedArgs\".",
                errors.Single(), 3, 30);
        }

        [Test]
        public void MultipleUnknownArguments_ReportsMultipleErrors()
        {
            var errors = Validate(@"
            fragment oneGoodArgTwoInvalidArgs on ComplicatedArgs {
                nonNullIntArgField(whoknows: ABC, nonNullIntArg: 1, unknown: true)
            }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual("Unknown argument \"whoknows\" on field \"nonNullIntArgField\" of type \"ComplicatedArgs\".",
                errors.ElementAt(0), 3, 36);
            ErrorAssert.AreEqual("Unknown argument \"unknown\" on field \"nonNullIntArgField\" of type \"ComplicatedArgs\".",
                errors.ElementAt(1), 3, 69);
        }

        [Test]
        public void DeeplyNestedUnknownArguments_ReportsMultipleErrors()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(unknown: true)
                }
                field(ab: ""yes"") {
                    complicatedArgs {
                        nested {
                            ... on ComplicatedArgs {
                                enumArgField(unknown: true)
                            }
                        }
                    }
                }
            }
            ");

            Assert.AreEqual(3, errors.Count());
            
            ErrorAssert.AreEqual("Unknown argument \"unknown\" on field \"enumArgField\" of type \"ComplicatedArgs\".",
                errors.ElementAt(0), 4, 34);
            ErrorAssert.AreEqual("Unknown argument \"ab\" on field \"field\" of type \"QueryRoot\". Did you mean \"a\" or \"b\"?",
                errors.ElementAt(1), 6, 23);
            ErrorAssert.AreEqual("Unknown argument \"unknown\" on field \"enumArgField\" of type \"ComplicatedArgs\".",
                errors.ElementAt(2), 10, 46);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new KnownArgumentNames()
                });
        }
    }
}
