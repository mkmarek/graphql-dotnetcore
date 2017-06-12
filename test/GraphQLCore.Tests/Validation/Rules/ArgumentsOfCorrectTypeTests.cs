namespace GraphQLCore.Tests.Validation.Rules
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

            Assert.IsEmpty(errors);
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

            ErrorAssert.AreEqual("Argument \"intArg\" has invalid value 829384293849283498239482938.\n" +
                "Expected type \"Int\", found 829384293849283498239482938.", errors.Single(), 4, 41);
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

            ErrorAssert.AreEqual("Argument \"stringArg\" has invalid value true.\n" +
                "Expected type \"String\", found true.", errors.Single(), 4, 47);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value true.\n" +
                "Expected type \"FurColor\", found true.", errors.Single(), 4, 43);
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

            ErrorAssert.AreEqual("Argument \"floatArg\" has invalid value true.\n" +
                "Expected type \"Float\", found true.", errors.Single(), 4, 45);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value TaN.\n" +
                "Expected type \"FurColor\", found TaN.", errors.Single(), 4, 43);
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

            Assert.IsEmpty(errors);
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

            ErrorAssert.AreEqual("Argument \"booleanArg\" has invalid value 3.14.\n" +
                "Expected type \"Boolean\", found 3.14.", errors.Single(), 4, 49);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value 3.14.\n" +
                "Expected type \"FurColor\", found 3.14.", errors.Single(), 4, 43);
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

            ErrorAssert.AreEqual("Argument \"intArg\" has invalid value 3.14.\n" +
                "Expected type \"Int\", found 3.14.", errors.Single(), 4, 41);
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

            ErrorAssert.AreEqual("Argument \"stringArg\" has invalid value 3.14.\n" +
                "Expected type \"String\", found 3.14.", errors.Single(), 4, 47);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            ErrorAssert.AreEqual("Argument \"stringListArg\" has invalid value [\"one\", 2].\n" +
                "In element #1: Expected type \"String\", found 2.", errors.Single(), 4, 55);
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

            ErrorAssert.AreEqual("Argument \"booleanArg\" has invalid value 1.\n" +
                "Expected type \"Boolean\", found 1.", errors.Single(), 4, 49);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value 1.\n" +
                "Expected type \"FurColor\", found 1.", errors.Single(), 4, 43);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            Assert.IsEmpty(errors);
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

            ErrorAssert.AreEqual("Argument \"intArg\" has invalid value 3.0.\n" +
                "Expected type \"Int\", found 3.0.", errors.Single(), 4, 41);
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

            ErrorAssert.AreEqual("Argument \"booleanArg\" has invalid value \"true\".\n" +
                "Expected type \"Boolean\", found \"true\".", errors.Single(), 4, 49);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value \"BAR\".\n" +
                "Expected type \"FurColor\", found \"BAR\".", errors.Single(), 4, 43);
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

            ErrorAssert.AreEqual("Argument \"floatArg\" has invalid value \"BAR\".\n" +
                "Expected type \"Float\", found \"BAR\".", errors.Single(), 4, 45);
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

            ErrorAssert.AreEqual("Argument \"intArg\" has invalid value \"BAR\".\n" +
                "Expected type \"Int\", found \"BAR\".", errors.Single(), 4, 41);
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

            ErrorAssert.AreEqual("Argument \"enumArg\" has invalid value NOT_KNOWN.\n" +
                "Expected type \"FurColor\", found NOT_KNOWN.", errors.Single(), 4, 43);
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

            ErrorAssert.AreEqual("Argument \"booleanArg\" has invalid value TRUE.\n" +
                "Expected type \"Boolean\", found TRUE.", errors.Single(), 4, 49);
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

            ErrorAssert.AreEqual("Argument \"floatArg\" has invalid value BAR.\n" +
                "Expected type \"Float\", found BAR.", errors.Single(), 4, 45);
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

            ErrorAssert.AreEqual("Argument \"intArg\" has invalid value BAR.\n" +
                "Expected type \"Int\", found BAR.", errors.Single(), 4, 41);
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

            ErrorAssert.AreEqual("Argument \"stringArg\" has invalid value BAR.\n" +
                "Expected type \"String\", found BAR.", errors.Single(), 4, 47);
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

            ErrorAssert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, intField: \"aaa\"}.\n" +
                "In field \"intField\": Expected type \"Int\", found \"aaa\".", errors.Single(), 3, 48);
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

            ErrorAssert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, stringListField: [null, 1, \"3\", [8, 5, 4]]}.\n" +
                "In field \"stringListField\": In element #1: Expected type \"String\", found 1.\n" +
                "In field \"stringListField\": In element #3: Expected type \"String\", found [8, 5, 4].",
                errors.Single(), 3, 48);
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

            ErrorAssert.AreEqual("Argument \"inputObject\" has invalid value {nonNullIntField: 0, nested: {nonNullIntField: null}}.\n" +
                "In field \"nested\": In field \"nonNullIntField\": Expected type \"Int!\", found null.",
                errors.Single(), 3, 48);
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

            ErrorAssert.AreEqual("Argument \"idArg\" has invalid value 1.0.\n" +
                "Expected type \"ID\", found 1.0.", errors.Single(), 4, 39);
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

            ErrorAssert.AreEqual("Argument \"idArg\" has invalid value true.\n" +
                "Expected type \"ID\", found true.", errors.Single(), 4, 39);
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

            ErrorAssert.AreEqual("Argument \"idArg\" has invalid value SOMETHING.\n" +
                "Expected type \"ID\", found SOMETHING.", errors.Single(), 4, 39);
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

            ErrorAssert.AreEqual("Argument \"if\" has invalid value \"yes\".\n" +
                "Expected type \"Boolean\", found \"yes\".", errors.ElementAt(0), 3, 46);
            ErrorAssert.AreEqual("Argument \"if\" has invalid value ENUM.\n" +
                "Expected type \"Boolean\", found ENUM.", errors.ElementAt(1), 4, 46);
        }
    }
}