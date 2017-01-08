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
    public class ArgumentsOfCorrectTypeTests : ValidationTestBase
    {
        [Test]
        public void ArgOnOptionalArg_ExpectsNoError()
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
        public void BigIntIntoInt_ExpectsSingleError()
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
        public void BooleanIntoString_ExpectsSingleError()
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
        public void BoolIntoEnum_ExpectsSingleError()
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
        public void BoolIntoFloat_ExpectsSingleError()
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
        public void ComplexObjectOnlyrequiredValues_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    complicatedObjectArgField(complicatedObjectArg: { nonNullIntField: 1, booleanField: true, enumField :TAN, floatField: 1.2 }) {
                        nonNullIntField
                    }
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void ComplexObjectOptinalArgNoValue_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    complicatedObjectArgField {
                       nonNullIntField
                    }
                }
            }
            ");

            Assert.IsFalse(errors.Any());
        }

        [Test]
        public void DifferentCaseEnum_ExpectsSingleError()
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
        public void EmptyListValue_ExpectsNoError()
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
        public void FloatIntoBool_ExpectsSingleError()
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
        public void FloatIntoEnum_ExpectsSingleError()
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
        public void FloatIntoInt_ExpectsSingleError()
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
        public void FloatIntoString_ExpectsSingleError()
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
        public void GoodBooleanValue_ExpectsNoError()
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
        public void GoodEnumValue_ExpectsNoError()
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
        public void GoodFloatValue_ExpectsNoError()
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
        public void GoodIntValue_ExpectsNoError()
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
        public void GoodListValue_ExpectsNoError()
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
        public void GoodStringValue_ExpectsNoError()
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
        public void IncorrectItemType_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    stringListArgField(stringListArg: " + "[\"one\", 2]" + @")
                }
            }
            ");

            Assert.AreEqual("Argument \"stringListArg\" has invalid value [\"one\", 2] In element #1: Expected type \"String\", found 2.",
                errors.Single().Message);
        }

        [Test]
        public void IntIntoBool_ExpectsSingleError()
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
        public void IntIntoEnum_ExpectsSingleError()
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
        public void IntIntoFloat_ExpectsNoError()
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
        public void MultipleArgs_ExpectsNoError()
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
        public void MultipleArgsReversedOrder_ExpectsNoError()
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
        public void NoArgOnOptionalArg_ExpectsNoError()
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
        public void NoArgsOnMultipleOptional_ExpectsNoError()
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
        public void SimpleFloatIntoInt_ExpectsSingleError()
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
        public void StringIntoBool_ExpectsSingleError()
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
        public void StringIntoEnum_ExpectsSingleError()
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
        public void StringIntoFloat_ExpectsSingleError()
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
        public void StringIntoInt_ExpectsSingleError()
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
        public void UnknownEnumValue_ExpectsSingleError()
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
        public void UnquotedIntoBool_ExpectsSingleError()
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
        public void UnquotedStringIntoFloat_ExpectsSingleError()
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
        public void UnquotedStringIntoInt_ExpectsSingleError()
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
        public void UnquotedStringIntoString_ExpectsSingleError()
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
    }
}