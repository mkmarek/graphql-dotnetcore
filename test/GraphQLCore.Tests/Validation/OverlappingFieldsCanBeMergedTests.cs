namespace GraphQLCore.Tests.Validation
{
    using Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    [TestFixture]
    public class OverlappingFieldsCanBeMergedTests : ValidationTestBase
    {
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

            Assert.AreEqual(
                "Fields \"fido\" conflict because name and nickname are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"name\" conflict because nickname and name are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"doesKnowCommand\" conflict because they have differing arguments" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(0).Message);

            Assert.AreEqual(
                "Fields \"x\" conflict because c and a are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(1).Message);

            Assert.AreEqual(
                "Fields \"x\" conflict because c and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.ElementAt(2).Message);
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

            Assert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are different fields and subfields \"y\" conflict because c and d are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"field\" conflict because subfields \"deepField\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"deepField\" conflict because subfields \"x\" conflict because a and b are different fields" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.", 
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"deeperField\" conflict because subfields \"x\" conflict because"+
                " a and b are different fields. Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"field\" conflict because subfields \"x\" conflict because a and b are " +
                "different fields and subfields \"y\" conflict because c and d are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"scalar\" conflict because they return conflicting types Int and String. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"other\" conflict because subfields \"scalar\" conflict because scalar and unrelatedField are different fields." +
                " Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"scalar\" conflict because they return conflicting types String! and String. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"box\" conflict because they return conflicting types [StringBox] and StringBox. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"box\" conflict because they return conflicting types StringBox and [StringBox]. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"val\" conflict because scalar and unrelatedField are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"box\" conflict because subfields \"scalar\" conflict because they return conflicting types String and Int. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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

            Assert.AreEqual(
                "Fields \"edges\" conflict because subfields \"node\" conflict because subfields \"id\""+
                " conflict because name and id are different fields. " +
                "Use different aliases on the fields to fetch both if this was intentional.",
                errors.Single().Message);
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
