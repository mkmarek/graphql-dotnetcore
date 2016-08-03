namespace GraphQLCore.Tests.Validation
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

            Assert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Variable \"$intArg\" of type \"Int\"" +
                " used in position expecting type \"Int!\".",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Variable \"$stringVar\" of type \"String\"" +
                " used in position expecting type \"Boolean\".",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Variable \"$stringVar\" of type \"String\"" +
                " used in position expecting type \"[String]\".",
                errors.Single().Message);
        }
    }
}
