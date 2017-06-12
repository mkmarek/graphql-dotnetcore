namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class OverlappingFieldsCanBeMergedTests : ValidationTestBase
    {
        [Test]
        public void FragmentCycles_NoError()
        {
            var errors = Validate(@"
                fragment uniqueFields on Dog {
                    name
                    nickname
                    ...uniqueFields
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void UniqueFields_ReportsNoError()
        {
            var errors = Validate(@"
                fragment uniqueFields on Dog {
                    name
                    nickname
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IdenticalFields_ReportsNoError()
        {
            var errors = Validate(@"
                fragment mergeIdenticalFields on Dog {
                    name
                    name
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IdenticalFieldsWithIdenticalArgs_ReportsNoError()
        {
            var errors = Validate(@"
                fragment mergeIdenticalFieldsWithIdenticalArgs on Dog {
                    doesKnowCommand(dogCommand: SIT)
                    doesKnowCommand(dogCommand: SIT)
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void IdenticalFieldsWithIdenticalDirectives_ReportsNoError()
        {
            var errors = Validate(@"
                fragment mergeSameFieldsWithSameDirectives on Dog {
                    name @include(if: true)
                    name @include(if: true)
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DifferentArgsWithDifferentAliases_ReportsNoError()
        {
            var errors = Validate(@"
                fragment differentArgsWithDifferentAliases on Dog {
                    knowsSit: doesKnowCommand(dogCommand: SIT)
                    knowsDown: doesKnowCommand(dogCommand: DOWN)
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DifferentDirectivesWithDifferentAliases_ReportsNoError()
        {
            var errors = Validate(@"
                fragment differentDirectivesWithDifferentAliases on Dog {
                    nameIfTrue: name @include(if: true)
                    nameIfFalse: name @include(if: false)
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void DifferentSkipIncludeDirectives_ReportsNoError()
        {
            var errors = Validate(@"
                fragment differentDirectivesWithDifferentAliases on Dog {
                    name @include(if: true)
                    name @include(if: false)
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameAliasesWithDifferentFieldTargets_ReportsError()
        {
            var errors = Validate(@"
                fragment sameAliasesWithDifferentFieldTargets on Dog {
                    fido: name
                    fido: nickname
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"fido\" conflict because name and nickname are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 3, 21 }, new[] { 4, 21 });
        }

        [Test]
        public void SameAliasesNonOverlappingFields_ReportsNoError()
        {
            var errors = Validate(@"
                fragment sameAliasesWithDifferentFieldTargets on Pet {
                    ... on Dog {
                    name
                    }
                    ... on Cat {
                    name: nickname
                    }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void AliasMaskingDirectFieldAccess_ReportsError()
        {
            var errors = Validate(@"
                fragment aliasMaskingDirectFieldAccess on Dog {
                    name: nickname
                    name
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"name\" conflict because nickname and name are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 3, 21 }, new[] { 4, 21 });
        }

        [Test]
        public void DifferentArgsSecondAddsAnArgument_ReportsError()
        {
            var errors = Validate(@"
                fragment conflictingArgs on Dog {
                    doesKnowCommand
                    doesKnowCommand(dogCommand: HEEL)
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 3, 21 }, new[] { 4, 21 });
        }

        [Test]
        public void DifferentArgsSecondMissingAnArgument_ReportsError()
        {
            var errors = Validate(@"
                fragment conflictingArgs on Dog {
                    doesKnowCommand(dogCommand: SIT)
                    doesKnowCommand
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 3, 21 }, new[] { 4, 21 });
        }

        [Test]
        public void ConflictingArguments_ReportsError()
        {
            var errors = Validate(@"
                fragment conflictingArgs on Dog {
                    doesKnowCommand(dogCommand: SIT)
                    doesKnowCommand(dogCommand: HEEL)
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 3, 21 }, new [] { 4, 21 });
        }

        [Test]
        public void DifferentArgumentsWhereNoConflictIsPossible_ReportsNoError()
        {
            var errors = Validate(@"
                fragment conflictingArgs on Pet {
                    ... on Dog {
                    name(surname: true)
                    }
                    ... on Cat {
                    name
                    }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void EncountersConflictInFragments_ReportsError()
        {
            var errors = Validate(@"
                {
                    ...A
                    ...B
                }
                fragment A on Type {
                    x: a
                }
                fragment B on Type {
                    x: b
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(), new[] { 7, 21 }, new[] { 10, 21 });
        }

        [Test]
        public void EncountersConflictInFragments_ReportsEachConflictOnce()
        {
            var errors = Validate(@"
                {
                    f1 {
                        ...A
                        ...B
                    }
                    f2 {
                        ...B
                        ...A
                    }
                    f3 {
                        ...A
                        ...B
                        x: c
                    }
                }
                fragment A on Type {
                    x: a
                }
                fragment B on Type {
                    x: b
                }
            ");

            Assert.AreEqual(3, errors.Count());

            ErrorAssert.AreEqual(
                "Fields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(0), new[] { 18, 21 }, new[] { 21, 21 });

            ErrorAssert.AreEqual(
                "Fields \"x\" conflict because c and a are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(1), new[] { 14, 25 }, new[] { 18, 21 });

            ErrorAssert.AreEqual(
                "Fields \"x\" conflict because c and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(2), new[] { 14, 25 }, new[] { 21, 21 });
        }

        [Test]
        public void DeepConflict_ReportsError()
        {
            var errors = Validate(@"
                {
                    field {
                        x: a
                    },
                    field {
                        x: b
                    }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(),
                new[] { 3, 21 },
                new[] { 4, 25 },
                new[] { 6, 21 },
                new[] { 7, 25 });
        }

        [Test]
        public void DeepConflictWithMultipleIssues_ReportsError()
        {
            var errors = Validate(@"
                {
                    field {
                        x: a
                        y: c
                    },
                    field {
                        x: b
                        y: d
                    }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are different fields and subfields \"y\" conflict because c and d are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(),
                new[] { 3, 21 },
                new[] { 4, 25 },
                new[] { 5, 25 },
                new[] { 7, 21 },
                new[] { 8, 25 },
                new[] { 9, 25 });
        }

        [Test]
        public void VeryDeepConflict_ReportsError()
        {
            var errors = Validate(@"
                {
                    field {
                        deepField {
                            x: a
                        }
                    },
                    field {
                        deepField {
                            x: b
                        }
                    }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"field\" conflict because subfields \"deepField\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(),
                new[] { 3, 21 },
                new[] { 4, 25 },
                new[] { 5, 29 },
                new[] { 8, 21 },
                new[] { 9, 25 },
                new[] { 10, 29 });
        }

        [Test]
        public void DeepConflict_ReportsToNearestCommonAncestor()
        {
            var errors = Validate(@"
                {
                    field {
                        deepField {
                            x: a
                        }
                        deepField {
                            x: b
                        }
                    },
                    field {
                        deepField {
                            y
                        }
                    }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"deepField\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single(),
                new[] { 4, 25 },
                new[] { 5, 29 },
                new[] { 7, 25 },
                new[] { 8, 29 });
        }

        [Test]
        public void DeepConflict_ReportsToNearestCommonAncestorInFragments()
        {
            var errors = Validate(@"
                {
                    field {
                    ...F
                    }
                    field {
                    ...F
                    }
                }
                fragment F on Type {
                    deepField {
                        deeperField {
                            x: a
                        }
                        deeperField {
                            x: b
                        }
                    },
                    deepField {
                        deeperField {
                            y
                        }
                    }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"deeperField\" conflict because subfields \"x\" conflict because"+
                " a and b are different fields. Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(),
                new[] { 12, 25 },
                new[] { 13, 29 },
                new[] { 15, 25 },
                new[] { 16, 29 });
        }

        [Test]
        public void DeepConflict_InNestedFragments()
        {
            var errors = Validate(@"
                {
                    field {
                        ...F
                    }
                    field {
                        ...I
                    }
                }
                fragment F on T {
                    x: a
                    ...G
                }
                fragment G on T {
                    y: c
                }
                fragment I on T {
                    y: d
                    ...J
                }
                fragment J on T {
                    x: b
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are " +
                "different fields and subfields \"y\" conflict because c and d are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(),
                new[] { 3, 21 },
                new[] { 11, 21 },
                new[] { 15, 21 },
                new[] { 6, 21 },
                new[] { 22, 21 },
                new[] { 18, 21 });
        }

        [Test]
        public void IgnoresUnknownFragments()
        {
            var errors = Validate(@"
                 {
                  field
                  ...Unknown
                  ...Known
                }
                fragment Known on T {
                  field
                  ...OtherUnknown
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ConflictingReturnTypeWhichPotentiallyOverlap()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ...on IntBox {
                      scalar
                    }
                    ...on StringBox  {
                      scalar
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"scalar\" conflict because they return conflicting types Int and String. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(), new[] { 5, 23 }, new[] { 8, 23 });
        }

        [Test]
        public void ReportsCorrectlyWhenNonExclusiveFollowsAnExclusive()
        {
            var errors = Validate(@"
                {
                  someBox {
                    ... on IntBox {
                      deepBox {
                        ...X
                      }
                    }
                  }
                  someBox {
                    ... on StringBox {
                      deepBox {
                        ...Y
                      }
                    }
                  }
                  memoed: someBox {
                    ... on IntBox {
                      deepBox {
                        ...X
                      }
                    }
                  }
                  memoed: someBox {
                    ... on StringBox {
                      deepBox {
                        ...Y
                      }
                    }
                  }
                  other: someBox {
                    ...X
                  }
                  other: someBox {
                    ...Y
                  }
                }
                fragment X on SomeBox {
                  scalar
                }
                fragment Y on SomeBox {
                  scalar: unrelatedField
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"other\" conflict because subfields \"scalar\" conflict because scalar and unrelatedField are different fields." +
                " Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(),
                new[] { 31, 19 },
                new[] { 39, 19 },
                new[] { 34, 19 },
                new[] { 42, 19 });
        }

        [Test]
        public void DisallowsDifferingReturnTypeNullabilityDespiteNoOverlap()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ... on NonNullStringBox1 {
                      scalar
                    }
                    ... on StringBox {
                      scalar
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"scalar\" conflict because they return conflicting types String! and String. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(), new[] { 5, 23 }, new[] { 8, 23 });
        }

        [Test]
        public void DisallowsDifferingReturnTypeListDespiteNoOverlap()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ... on IntBox {
                      box: listStringBox {
                        scalar
                      }
                    }
                    ... on StringBox {
                      box: stringBox {
                        scalar
                      }
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"box\" conflict because they return conflicting types [StringBox] and StringBox. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(), new[] { 5, 23 }, new[] { 10, 23 });
        }

        [Test]
        public void DisallowsDifferingReturnTypeListDespiteNoOverlap_Reversed()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ... on IntBox {
                      box: stringBox {
                        scalar
                      }
                    }
                    ... on StringBox {
                      box: listStringBox {
                        scalar
                      }
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"box\" conflict because they return conflicting types StringBox and [StringBox]. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(), new[] { 5, 23 }, new[] { 10, 23 });
        }

        [Test]
        public void DisallowsDifferingSubfields()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ... on IntBox {
                      box: stringBox {
                        val: scalar
                        val: unrelatedField
                      }
                    }
                    ... on StringBox {
                      box: stringBox {
                        val: scalar
                      }
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"val\" conflict because scalar and unrelatedField are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(), new[] { 6, 25 }, new[] { 7, 25 });
        }

        [Test]
        public void DisallowsDifferingDeepReturnTypesDespiteNoOverlap()
        {
            var errors = Validate(@"
                {
                  someBox {
                    ... on IntBox {
                      box: stringBox {
                        scalar
                      }
                    }
                    ... on StringBox {
                      box: intBox {
                        scalar
                      }
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"box\" conflict because subfields \"scalar\" conflict because they return conflicting types String and Int. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(),
                new[] { 5, 23 },
                new[] { 6, 25 },
                new[] { 10, 23 },
                new[] { 11, 25 });
        }

        [Test]
        public void AllowsNonConflictingOverlappingTypes()
        {
            var errors = Validate(@"
                {
                  someBox {
                    ... on IntBox {
                      scalar: unrelatedField
                    }
                    ... on StringBox {
                      scalar
                    }
                  }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SameWrappedScalarReturnTypes()
        {
            var errors = Validate(@"
                 {
                  someBox {
                    ...on NonNullStringBox1 {
                      scalar
                    }
                    ...on NonNullStringBox2 {
                      scalar
                    }
                  }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void AllowsInlineTypelessFragments()
        {
            var errors = Validate(@"
                  {
                    a
                    ... {
                    a
                    }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void ComparesDeepTypesIncludingLists()
        {
            var errors = Validate(@"
                 {
                  connection {
                    ...edgeID
                    edges {
                      node {
                        id: name
                      }
                    }
                  }
                }
                fragment edgeID on Connection {
                  edges {
                    node {
                      id
                    }
                  }
                }
            ");

            ErrorAssert.AreEqual(
                "Fields \"edges\" conflict because subfields \"node\" conflict because subfields \"id\""+
                " conflict because name and id are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single(),
                new[] { 5, 21 },
                new[] { 6, 23 },
                new[] { 7, 25 },
                new[] { 13, 19 },
                new[] { 14, 21 },
                new[] { 15, 23 });
        }

        [Test]
        public void IgnoresUnknownTypes()
        {
            var errors = Validate(@"
                  {
                  someBox {
                    ...on UnknownType {
                      scalar
                    }
                    ...on NonNullStringBox2 {
                      scalar
                    }
                  }
                }
            ");

            Assert.IsEmpty(errors);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new OverlappingFieldsCanBeMerged()
                });
        }
    }
}
