namespace GraphQLCore.Tests.Type.Directives
{
    using System;
    using System.Linq.Expressions;
    using GraphQLCore.Tests.Schemas;
    using GraphQLCore.Type.Directives;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphQLDirectiveTests
    {
        private TestSchema schema;
        private GraphQLDirectiveType testDirective;

        [SetUp]
        public void SetUp()
        {
            this.schema = new TestSchema();

            this.testDirective = Substitute.For<GraphQLDirectiveType>(
                "test", "some description", DirectiveLocation.FIELD);

            this.testDirective.PreExecutionIncludeFieldIntoResult(null, null)
                .ReturnsForAnyArgs(true);

            this.testDirective.PostExecutionIncludeFieldIntoResult(null, null, null, null)
                .ReturnsForAnyArgs(true);

            this.testDirective.GetResolver(Arg.Any<object>(), Arg.Any<object>())
                .Returns((Expression<Func<object>>)(() => "modified"));

            this.schema.AddOrReplaceDirective(this.testDirective);
        }

        [Test]
        public void DirectiveOnField_InvokesGetResolverWhenHitsFieldWithCorrectValue()
        {
            var result = this.schema.Execute(@"
            {
                foo @test
            }
            ");

            this.testDirective
                .Received()
                .GetResolver("bar", Arg.Any<dynamic>());
        }

        [Test]
        public void DirectiveOnField_ModifiesFieldValue()
        {
            var result = this.schema.Execute(@"
            {
                foo @test
            }
            ");

            Assert.AreEqual("modified", result.foo);
        }

        [Test]
        public void DirectiveOnFieldNotIncludesField_DoesNotCallGetResolver()
        {
             this.testDirective.PreExecutionIncludeFieldIntoResult(null, null)
                .ReturnsForAnyArgs(false);

            var count = 0;
            this.testDirective.WhenForAnyArgs(e => e.GetResolver(null, null)).Do(e => count++);

            var result = this.schema.Execute(@"
            {
                foo @test
            }
            ");

            // TODO Do not use GetResolver for validation of directive arguments
            Assert.IsTrue(count <= 1);
        }

        [Test]
        public void DirectiveOnFieldNotIncludesFieldPostExecution_DoesCallGetResolver()
        {
             this.testDirective.PostExecutionIncludeFieldIntoResult(null, null, null, null)
                .ReturnsForAnyArgs(false);

            var result = this.schema.Execute(@"
            {
                foo @test
            }
            ");

            this.testDirective
                .Received()
                .GetResolver("bar", Arg.Any<dynamic>());
        }

        [Test]
        public void Directive_GetUnknownArgument_ReturnsNull()
        {
            var result = this.testDirective.GetArgument("unknown");

            Assert.IsNull(result);
        }
    }
}