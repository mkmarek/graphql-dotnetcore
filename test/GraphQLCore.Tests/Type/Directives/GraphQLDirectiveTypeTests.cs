namespace GraphQLCore.Tests.Type.Directives
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reactive.Linq;
    using GraphQLCore.Tests.Schemas;
    using GraphQLCore.Type.Directives;
    using NSubstitute;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using GraphQLCore.Execution;

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

            this.testDirective.GetResolver(Arg.Any<Func<Task<object>>>(), Arg.Any<object>())
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
                .GetResolver(Arg.Any<Func<Task<object>>>(), Arg.Any<dynamic>());
        }

        [Test]
        public void DirectiveOnField_ModifiesFieldValue()
        {
            var result = this.schema.Execute(@"
            {
                foo @test
            }
            ");

            Assert.AreEqual("modified", result.Data.foo);
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

            Assert.IsTrue(count == 0);
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
                .GetResolver(Arg.Any<Func<Task<object>>>(), Arg.Any<dynamic>());
        }

        [Test]
        public void Directive_GetUnknownArgument_ReturnsNull()
        {
            var result = this.testDirective.GetArgument("unknown");

            Assert.IsNull(result);
        }

        [Test]
        public async Task DirectiveOnField_Postpone_PostponesExecution()
        {
            this.testDirective.PostponeNodeResolve().Returns(true);

            var subscription = this.schema.Subscribe(@"
            {
                foo @test
            }
            ");

            var result = await subscription.ToList();
            var postponed = result[1] as PartialExecutionResult;

            Assert.AreEqual(2, result.Count);
            Assert.IsEmpty(result[0].Data);
            Assert.IsNull(result[0].Errors);

            Assert.AreEqual("modified", postponed.Data);
            Assert.IsNull(postponed.Errors);
            Assert.AreEqual(new[] { "foo" }, postponed.Path);
        }
    }
}
