namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Language;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Validation;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using Schemas;
    using System.Linq;

    [TestFixture]
    public class ArgumentsOfCorrectTypeTests
    {
        private ArgumentsOfCorrectType argumentsOfCorrectType;
        private ValidationContext validationContext;
        private TestSchema validationTestSchema;

        [SetUp]
        public void SetUp()
        {
            this.argumentsOfCorrectType = new ArgumentsOfCorrectType();
            this.validationTestSchema = new TestSchema();

            this.validationContext = new ValidationContext();
        }

        [Test]
        public void Validate_ArgOnOptionalArg_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 1)
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_BigIntIntoInt_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 829384293849283498239482938)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"intArg\" has invalid value 829384293849283498239482938"));
        }

        [Test]
        public void Validate_BooleanIntoString_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: true)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"stringArg\" has invalid value true"));
        }

        [Test]
        public void Validate_BoolIntoEnum_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: true)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value true"));
        }

        [Test]
        public void Validate_BoolIntoFloat_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: true)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"floatArg\" has invalid value true"));
        }

        [Test]
        public void Validate_ComplexObjectOnlyrequiredValues_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    complicatedObjectArgField(complicatedObjectArg: { nonNullIntField: 1, booleanField: true, enumField :TAN, floatField: 1.2 })
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_ComplexObjectOptinalArgNoValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    complicatedObjectArgField
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_DifferentCaseEnum_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: TaN)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value TaN"));
        }

        [Test]
        public void Validate_EmptyListValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringListArgField(stringListArg: [])
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_FloatIntoBool_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: 3.14)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"booleanArg\" has invalid value 3.14"));
        }

        [Test]
        public void Validate_FloatIntoEnum_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: 3.14)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value 3.14"));
        }

        [Test]
        public void Validate_FloatIntoInt_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 3.14)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"intArg\" has invalid value 3.14"));
        }

        [Test]
        public void Validate_FloatIntoString_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: 3.14)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"stringArg\" has invalid value 3.14"));
        }

        [Test]
        public void Validate_GoodBooleanValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: true)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodEnumValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: TAN)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodFloatValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: 1.2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodIntValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_GoodListValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringListArgField(stringListArg: " + "[\"one\", \"two\"]" + @")
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_GoodStringValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: " + "\"foo\"" + @")
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_IncorrectItemType_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringListArgField(stringListArg: " + "[\"one\", 2]" + @")
                }
            }
            ");

            Assert.AreEqual("Argument \"stringListArg\" has invalid value [\"one\", 2] In element #1: Expected type \"String\", found 2",
                errors.Single().Message);
        }

        [Test]
        public void Validate_IntIntoBool_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: 1)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"booleanArg\" has invalid value 1"));
        }

        [Test]
        public void Validate_IntIntoEnum_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: 1)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value 1"));
        }

        [Test]
        public void Validate_IntIntoFloat_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: 2)
                }
            }
            ");

            Assert.AreEqual(0, errors.Count());
        }

        [Test]
        public void Validate_MultipleArgs_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    multipleArgsField(arg1: 1, arg2: 2)
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_MultipleArgsReversedOrder_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    multipleArgsField(arg2: 1, arg1: 2)
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_NoArgOnOptionalArg_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_NoArgsOnMultipleOptional_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    multipleArgsField
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void Validate_SimpleFloatIntoInt_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: 3.0)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"intArg\" has invalid value 3.0"));
        }

        [Test]
        public void Validate_StringIntoBool_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: " + "\"true\"" + @")
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"booleanArg\" has invalid value \"true\""));
        }

        [Test]
        public void Validate_StringIntoEnum_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: " + "\"BAR\"" + @")
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value \"BAR\""));
        }

        [Test]
        public void Validate_StringIntoFloat_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: " + "\"BAR\"" + @")
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"floatArg\" has invalid value \"BAR\""));
        }

        [Test]
        public void Validate_StringIntoInt_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: " + "\"BAR\"" + @")
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"intArg\" has invalid value \"BAR\""));
        }

        [Test]
        public void Validate_UnknownEnumValue_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    enumArgField(enumArg: NOT_KNOWN)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"enumArg\" has invalid value NOT_KNOWN"));
        }

        [Test]
        public void Validate_UnquotedIntoBool_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    booleanArgField(booleanArg: TRUE)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"booleanArg\" has invalid value TRUE"));
        }

        [Test]
        public void Validate_UnquotedStringIntoFloat_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    floatArgField(floatArg: BAR)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"floatArg\" has invalid value BAR"));
        }

        [Test]
        public void Validate_UnquotedStringIntoInt_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    intArgField(intArg: BAR)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"intArg\" has invalid value BAR"));
        }

        [Test]
        public void Validate_UnquotedStringIntoString_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringArgField(stringArg: BAR)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"stringArg\" has invalid value BAR"));
        }

        private static GraphQLDocument GetAst(string body)
        {
            return new Parser(new Lexer()).Parse(new Source(body));
        }

        private GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[] { this.argumentsOfCorrectType });
        }
    }
}