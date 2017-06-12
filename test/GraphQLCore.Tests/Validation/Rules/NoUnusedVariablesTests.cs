namespace GraphQLCore.Tests.Validation.Rules
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class NoUnusedVariablesTests : ValidationTestBase
    {
        [Test]
        public void NoUnusedVariables_UsesAllVariables()
        {
            var errors = Validate(@"
                query ($a: String, $b: String, $c: String) {
                    field(a: $a, b: $b, c: $c) { foo }
                }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NoUnusedVariables_UsesAllVariablesDeeply()
        {
            var errors = Validate(@"
                query Foo($a: String, $b: String, $c: String) {
                    field(a: $a) {
                      field(b: $b) {
                        field(c: $c) { foo }
                      }
                    }
                  }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NoUnusedVariables_UsesAllVariablesInFragments()
        {
            var errors = Validate(@"
                query Foo($a: String, $b: String, $c: String) {
                    ...FragA
                  }
                  fragment FragA on QueryRoot {
                    field(a: $a) {
                      ...FragB
                    }
                  }
                  fragment FragB on QueryRoot {
                    field(b: $b) {
                      ...FragC
                    }
                  }
                  fragment FragC on QueryRoot {
                    field(c: $c) { foo }
                  }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NoUnusedVariables_UsesAllVariablesInFragmentsInMultipleOperations()
        {
            var errors = Validate(@"
                query Foo($a: String) {
                    ...FragA
                  }
                  query Bar($b: String) {
                    ...FragB
                  }
                  fragment FragA on QueryRoot {
                    field(a: $a) { foo }
                  }
                  fragment FragB on QueryRoot {
                    field(b: $b) { foo }
                  }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void NoUnusedVariables_VariableUsedByRecursiveFragment()
        {
            var errors = Validate(@"
                query Foo($a: String) {
                ...FragA
              }
              fragment FragA on QueryRoot {
                field(a: $a) {
                  ...FragA
                }
              }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void VariableNotUsed()
        {
            var errors = Validate(@"
               query ($a: String, $b: String, $c: String) {
                field(a: $a, b: $b) { foo }
              }
            ");

            ErrorAssert.AreEqual($"Variable \"$c\" is never used.", errors.Single(), 2, 47);
        }

        [Test]
        public void VariableNotUsedInFragments()
        {
            var errors = Validate(@"
               query Foo($a: String, $b: String, $c: String) {
                ...FragA
              }
              fragment FragA on QueryRoot {
                field(a: $a) {
                  ...FragB
                }
              }
              fragment FragB on QueryRoot {
                field(b: $b) {
                  ...FragC
                }
              }
              fragment FragC on QueryRoot {
                field
              }
            ");

            ErrorAssert.AreEqual($"Variable \"$c\" is never used in operation \"Foo\".", errors.Single(), 2, 50);
        }

        [Test]
        public void MultipleVariablesNotUsedInFragments()
        {
            var errors = Validate(@"
               query Foo($a: String, $b: String, $c: String) {
                ...FragA
              }
              fragment FragA on QueryRoot {
                field {
                  ...FragB
                }
              }
              fragment FragB on QueryRoot {
                field(b: $b) {
                  ...FragC
                }
              }
              fragment FragC on QueryRoot {
                field
              }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual($"Variable \"$a\" is never used in operation \"Foo\".", errors.ElementAt(0), 2, 26);
            ErrorAssert.AreEqual($"Variable \"$c\" is never used in operation \"Foo\".", errors.ElementAt(1), 2, 50);
        }

        [Test]
        public void VariableNotUsedByUnreferencedFragment()
        {
            var errors = Validate(@"
               query Foo($b: String) {
                ...FragA
                }
                fragment FragA on QueryRoot {
                field(a: $a)
                }
                fragment FragB on QueryRoot {
                field(b: $b)
                }
            ");

            ErrorAssert.AreEqual($"Variable \"$b\" is never used in operation \"Foo\".", errors.Single(), 2, 26);
        }

        [Test]
        public void VariableNotUsedInFragmentUsedByOtherOperation()
        {
            var errors = Validate(@"
               query Foo($b: String) {
                ...FragA
              }
              query Bar($a: String) {
                ...FragB
              }
              fragment FragA on QueryRoot {
                field(a: $a)
              }
              fragment FragB on QueryRoot {
                field(b: $b)
              }
            ");

            Assert.AreEqual(2, errors.Count());

            ErrorAssert.AreEqual($"Variable \"$b\" is never used in operation \"Foo\".", errors.ElementAt(0), 2, 26);
            ErrorAssert.AreEqual($"Variable \"$a\" is never used in operation \"Bar\".", errors.ElementAt(1), 5, 25);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new NoUnusedVariables()
                });
        }
    }
}
