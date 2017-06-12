namespace GraphQLCore.Tests.Validation.Rules
{
    using NUnit.Framework;
    using System.Linq;

    public class VariablesInAllowedPositionsTests : ValidationTestBase
    {
        [Test]
        public void BooleanIntoBoolean()
        {
            var errors = this.Validate(@"
            query Query($booleanArg: Boolean)
            {
                complicatedArgs {
                    booleanArgField(booleanArg: $booleanArg)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void BooleanIntoBooleanWithinFragment()
        {
            var errors = this.Validate(@"
            fragment booleanArgFrag on ComplicatedArgs {
                booleanArgField(booleanArg: $booleanArg)
            }
            query Query($booleanArg: Boolean)
            {
                complicatedArgs {
                    ...booleanArgFrag
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NonNullBooleanIntoBoolean()
        {
            var errors = this.Validate(@"
            query Query($nonNullBooleanArg: Boolean!)
            {
                complicatedArgs {
                    booleanArgField(booleanArg: $nonNullBooleanArg)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NonNullBooleanIntoBooleanWithinFragment()
        {
            var errors = this.Validate(@"
            fragment booleanArgFrag on ComplicatedArgs {
                booleanArgField(booleanArg: $nonNullBooleanArg)
            }
            query Query($nonNullBooleanArg: Boolean)
            {
                complicatedArgs {
                    ...booleanArgFrag
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IntToNonNullIntWithDefaultValue()
        {
            var errors = this.Validate(@"
            query Query($intArg: Int = 1)
            {
                complicatedArgs {
                    nonNullIntArgField(nonNullIntArg: $intArg)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void StringArrayToStringArray()
        {
            var errors = this.Validate(@"
            query Query($stringListVar: [String])
            {
                complicatedArgs {
                    stringListArgField(stringListArg: $stringListVar)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void StringArrayWithNonNullMembersToStringArray()
        {
            var errors = this.Validate(@"
            query Query($stringListVar: [String!])
            {
                complicatedArgs {
                    stringListArgField(stringListArg: $stringListVar)
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void StringToStringArrayAsMember()
        {
            var errors = this.Validate(@"
            query Query($stringListVar: String)
            {
                complicatedArgs {
                    stringListArgField(stringListArg: [$stringListVar])
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NonNullStringToStringArrayAsMemberType()
        {
            var errors = this.Validate(@"
            query Query($stringListVar: String!)
            {
                complicatedArgs {
                    stringListArgField(stringListArg: [$stringListVar])
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ComplexInputToComplexInput()
        {
            var errors = this.Validate(@"
            query Query($complexVar: ComplicatedInputObjectType)
            {
                complicatedArgs {
                    complicatedObjectArgField(complexArg: $complexVar) {
                        intField
                    }
                }
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IntToNonNullInt()
        {
            var errors = this.Validate(@"
            query Query($intArg: Int) {
                complicatedArgs {
                    nonNullIntArgField(nonNullIntArg: $intArg)
                }
            }
            ");

            ErrorAssert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single(), new[] { 2, 25 }, new[] { 4, 55 });
        }

        [Test]
        public void IntToNonNullIntWithinFragment()
        {
            var errors = this.Validate(@"
            fragment nonNullIntArgFieldFrag on ComplicatedArgs {
                nonNullIntArgField(nonNullIntArg: $intArg)
            }
            query Query($intArg: Int) {
                complicatedArgs {
                ...nonNullIntArgFieldFrag
                }
            }
            ");

            ErrorAssert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single(), new[] { 5, 25 }, new[] { 3, 51 });
        }

        [Test]
        public void IntToNonNullIntWithinNestedFragment()
        {
            var errors = this.Validate(@"
            fragment outerFrag on ComplicatedArgs {
                ...nonNullIntArgFieldFrag
            }
            fragment nonNullIntArgFieldFrag on ComplicatedArgs {
                nonNullIntArgField(nonNullIntArg: $intArg)
            }
            query Query($intArg: Int) {
                complicatedArgs {
                    ...outerFrag
                }
            }
            ");

            ErrorAssert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single(), new[] { 8, 25 }, new[] { 6, 51 });
        }

        [Test]
        public void StringOverBoolean()
        {
            var errors = this.Validate(@"
            query Query($stringVar: String) {
                complicatedArgs {
                    booleanArgField(booleanArg: $stringVar)
                }
            }
            ");

            ErrorAssert.AreEqual(
                "Variable \"$stringVar\" of type \"String\"" +
                " used in position expecting type \"Boolean\".",
                errors.Single(), new[] { 2, 25 }, new[] { 4, 49 });
        }

        [Test]
        public void StringToStringArray()
        {
            var errors = this.Validate(@"
            query Query($stringVar: String) {
                complicatedArgs {
                    stringListArgField(stringListArg: $stringVar)
                }
            }
            ");

            ErrorAssert.AreEqual(
                "Variable \"$stringVar\" of type \"String\"" +
                " used in position expecting type \"[String]\".",
                errors.Single(), new[] { 2, 25 }, new[] { 4, 55 });
        }
    }
}
