namespace GraphQLCore.Tests.Validation
{
    using NUnit.Framework;
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

            Assert.AreEqual("Argument \"stringListArg\" has invalid value [\"one\", 2].\nIn element #1: Expected type \"String\", found 2.",
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

        [Test]
        public void StringIntoInt_InObjectField_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    intField: ""aaa""
                }) {
                    intField
                }
            }
            ");

            Assert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, intField: \"aaa\"}.\nIn field \"intField\": Expected type \"Int\", found \"aaa\".",
                errors.Single().Message);
        }

        [Test]
        public void IncorrectItemType_InListFieldInObject_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    stringListField: [null, 1, ""3"", [8, 5, 4]]
                }) {
                    stringListField
                }
            }
            ");

            Assert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, stringListField: [null, 1, \"3\", [8, 5, 4]]}.\nIn field \"stringListField\": In element #1: Expected type \"String\", found 1.\nIn field \"stringListField\": In element #3: Expected type \"String\", found [8, 5, 4].",
                errors.Single().Message);
        }

        [Test]
        public void IncorrectItemType_InNestedObjectField_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    nested: {
                        nonNullIntField: null
                    }
                }) {
                    nested {
                        nonNullIntField
                    }
                }
            }
            ");

            Assert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, nested: {nonNullIntField: null}}.\nIn field \"nested\": In field \"nonNullIntField\": Expected type \"Int!\", found null.",
                errors.Single().Message);
        }

        [Test]
        public void StringIntoID_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: ""someIdString"")
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IntIntoID_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: 1)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void BigIntIntoID_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: 829384293849283498239482938)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void FloatIntoID_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: 1.0)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"idArg\" has invalid value 1.0."));
        }

        [Test]
        public void BooleanIntoID_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: true)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"idArg\" has invalid value true."));
        }

        [Test]
        public void UnquotedStringIntoID_ExpectsSingleError()
        {
            var errors = Validate(@"
            {
                complicatedArgs {
                    idArgField(idArg: SOMETHING)
                }
            }
            ");

            Assert.IsTrue(errors.Single().Message.StartsWith("Argument \"idArg\" has invalid value SOMETHING."));
        }

        [Test]
        public void DirectivesOfCorrectTypes_ExpectsNoError()
        {
            var errors = Validate(@"
            {
                foo @include(if: true)
                bar @skip(if: false)
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DirectivesOfIncorrectTypes_ExpectsMultipleErrors()
        {
            var errors = Validate(@"
            {
                complicatedArgs @include(if: ""yes"") {
                    stringArgField @skip(if: ENUM)
                }
            }
            ");

            Assert.AreEqual(2, errors.Count());
            Assert.AreEqual("Argument \"if\" has invalid value \"yes\".\nExpected type \"Boolean\", found \"yes\".", errors.ElementAt(0).Message);
            Assert.AreEqual("Argument \"if\" has invalid value ENUM.\nExpected type \"Boolean\", found ENUM.", errors.ElementAt(1).Message);
        }
    }
}