namespace GraphQLCore.Tests.Validation
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Validation.Rules;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class KnownDirectivesTests : ValidationTestBase
    {
        [Test]
        public void NoDirectives_ReportsNoError()
        {
            var errors = Validate(@"
            query Foo {
                foo
                complicatedArgs {
                    ...Frag
                }
            }

            fragment Frag on ComplicatedArgs {
                intArgField
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void KnownDirectives_ReportsNoError()
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
        public void UnknownDirective_ReportsSingleError()
        {
            var errors = Validate(@"
            {
                foo @unknown(directive: ""value"")
            }
            ");

            Assert.AreEqual("Unknown directive \"unknown\".", errors.Single().Message);
        }

        [Test]
        public void MultipleUnknownDirectives_ReportsMultipleErrors()
        {
            var errors = Validate(@"
            {
                foo @unknown(directive: ""value"")
                complicatedArgs @unknown(directive: ""value"")
                {
                    intField
                    nested @unknown(directive: ""value"") {
                        intField
                    }
                }
            }
            ");

            Assert.AreEqual(3, errors.Count());
            Assert.AreEqual("Unknown directive \"unknown\".", errors.ElementAt(0).Message);
            Assert.AreEqual("Unknown directive \"unknown\".", errors.ElementAt(1).Message);
            Assert.AreEqual("Unknown directive \"unknown\".", errors.ElementAt(2).Message);
        }

        [Test]
        public void MultipleWellPlacedDirectives_ReportsNoError()
        {
            var errors = Validate(@"
            query Foo @onQuery {
                foo @include(if: true)
                ...Frag @include(if: true)
                bar @skip(if: true)
                ...SkippedFrag @skip(if: true)
            }

            mutation Bar @onMutation {
                insertInputObject {
                    intField
                }
            }

            fragment Frag on ComplicatedInterfaceType @onFragmentDefinition {
                booleanField
            }
            ");
            
            Assert.IsEmpty(errors);
        }

        [Test]
        public void MultipleMisplacedDirectives_ReportsMultipleErrors()
        {
            var errors = Validate(@"
            query Foo @include(if: true) {
                foo @onQuery
                ...Frag @onQuery
                ... @onField {
                    name
                }
            }

            mutation Bar @onQuery {
                insertInputObject {
                    intField
                }
            }

            subscription Sub @onMutation {
                __typename
            }
            ");

            Assert.AreEqual(6, errors.Count());
            Assert.AreEqual("Directive \"include\" may not be used on QUERY.", errors.ElementAt(0).Message);
            Assert.AreEqual("Directive \"onQuery\" may not be used on FIELD.", errors.ElementAt(1).Message);
            Assert.AreEqual("Directive \"onQuery\" may not be used on FRAGMENT_SPREAD.", errors.ElementAt(2).Message);
            Assert.AreEqual("Directive \"onField\" may not be used on INLINE_FRAGMENT.", errors.ElementAt(3).Message);
            Assert.AreEqual("Directive \"onQuery\" may not be used on MUTATION.", errors.ElementAt(4).Message);
            Assert.AreEqual("Directive \"onMutation\" may not be used on SUBSCRIPTION.", errors.ElementAt(5).Message);
        }

        [Test]
        public void SchemaLanguage_WellPlacedDirectives_ReportsNoError()
        {
            var errors = Validate(@"
            type MyObj implements MyInterface @onObject {
                myField(myArg: Int @onArgumentDefinition): String @onFieldDefinition
            }

            scalar MyScalar @onScalar

            interface MyInterface @onInterface {
                myField(myArg: Int @onArgumentDefinition): String @onFieldDefinition
            }

            union MyUnion @onUnion = MyObj | Other

            enum MyEnum @onEnum {
                MY_VALUE @onEnumValue
            }

            input MyInput @onInputObject {
                myField: Int @onInputFieldDefinition
            }

            schema @onSchema {
                query: MyQuery
            }
            ");

            Assert.IsEmpty(errors);
        }

        [Test]
        public void SchemaLanguage_MisplacedDirectives_ReportsMultipleErrors()
        {
            var errors = Validate(@"
            type MyObj implements MyInterface @onInterface {
                myField(myArg: Int @onInputFieldDefinition): String @onInputFieldDefinition
            }

            scalar MyScalar @onEnum

            interface MyInterface @onObject {
                myField(myArg: Int @onInputFieldDefinition): String @onInputFieldDefinition
            }

            union MyUnion @onEnumValue = MyObj | Other

            enum MyEnum @onScalar {
                MY_VALUE @onUnion
            }

            input MyInput @onEnum {
                myField: Int @onArgumentDefinition
            }

            schema @onObject {
                query: MyQuery
            }
            ");

            Assert.AreEqual(13, errors.Count());
            Assert.AreEqual("Directive \"onInterface\" may not be used on OBJECT.", errors.ElementAt(0).Message);
            Assert.AreEqual("Directive \"onInputFieldDefinition\" may not be used on ARGUMENT_DEFINITION.", errors.ElementAt(1).Message);
            Assert.AreEqual("Directive \"onInputFieldDefinition\" may not be used on FIELD_DEFINITION.", errors.ElementAt(2).Message);
            Assert.AreEqual("Directive \"onEnum\" may not be used on SCALAR.", errors.ElementAt(3).Message);
            Assert.AreEqual("Directive \"onObject\" may not be used on INTERFACE.", errors.ElementAt(4).Message);
            Assert.AreEqual("Directive \"onInputFieldDefinition\" may not be used on ARGUMENT_DEFINITION.", errors.ElementAt(5).Message);
            Assert.AreEqual("Directive \"onInputFieldDefinition\" may not be used on FIELD_DEFINITION.", errors.ElementAt(6).Message);
            Assert.AreEqual("Directive \"onEnumValue\" may not be used on UNION.", errors.ElementAt(7).Message);
            Assert.AreEqual("Directive \"onScalar\" may not be used on ENUM.", errors.ElementAt(8).Message);
            Assert.AreEqual("Directive \"onUnion\" may not be used on ENUM_VALUE.", errors.ElementAt(9).Message);
            Assert.AreEqual("Directive \"onEnum\" may not be used on INPUT_OBJECT.", errors.ElementAt(10).Message);
            Assert.AreEqual("Directive \"onArgumentDefinition\" may not be used on INPUT_FIELD_DEFINITION.", errors.ElementAt(11).Message);
            Assert.AreEqual("Directive \"onObject\" may not be used on SCHEMA.", errors.ElementAt(12).Message);
        }

        protected override GraphQLException[] Validate(string body)
        {
            return validationContext.Validate(
                GetAst(body),
                this.validationTestSchema,
                new IValidationRule[]
                {
                    new KnownDirectives()
                });
        }
    }
}
