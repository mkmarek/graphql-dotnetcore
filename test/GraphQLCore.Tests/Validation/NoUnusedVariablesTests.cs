namespace GraphQLCore.Tests.Validation
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

            Assert.AreEqual($"Variable \"$c\" is never used.", errors.Single().Message);
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

            Assert.AreEqual($"Variable \"$c\" is never used in operation \"Foo\".", errors.Single().Message);
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

            Assert.AreEqual($"Variable \"$a\" is never used in operation \"Foo\".", errors.First().Message);
            Assert.AreEqual($"Variable \"$c\" is never used in operation \"Foo\".", errors.Last().Message);
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

            Assert.AreEqual($"Variable \"$b\" is never used in operation \"Foo\".", errors.Single().Message);
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

            Assert.AreEqual($"Variable \"$b\" is never used in operation \"Foo\".", errors.First().Message);
            Assert.AreEqual($"Variable \"$a\" is never used in operation \"Bar\".", errors.Last().Message);
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
