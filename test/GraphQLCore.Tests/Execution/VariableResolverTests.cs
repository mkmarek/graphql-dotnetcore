namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Execution;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type.Translation;
    using NSubstitute;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Dynamic;

    public class VariableResolverTests
    {
        private GraphQLNamedType intNamedType;
        private ITypeTranslator typeTranslator;
        private VariableResolver variableResolver;

        [Test]
        public void GetValue_ScalarIntVariable_CallsTypeTranslatorWithCorrectType()
        {
            object value = this.variableResolver.GetValue("scalarIntVariable");

            this.typeTranslator.Received().GetType(this.intNamedType);
        }

        [SetUp]
        public void SetUp()
        {
            dynamic variables = new ExpandoObject();
            variables.scalarIntVariable = "1";

            this.intNamedType = GetIntNamedType();
            this.typeTranslator = Substitute.For<ITypeTranslator>();
            this.variableResolver = new VariableResolver(variables, this.typeTranslator, this.GetVariableDefinitions());
        }

        private static GraphQLNamedType GetIntNamedType()
        {
            return new GraphQLNamedType()
            {
                Name = new GraphQLName()
                {
                    Value = "Int"
                },
            };
        }

        private IEnumerable<GraphQLVariableDefinition> GetVariableDefinitions()
        {
            var definitions = new List<GraphQLVariableDefinition>();

            definitions.Add(new GraphQLVariableDefinition()
            {
                Type = this.intNamedType,
                Variable = new GraphQLVariable()
                {
                    Name = new GraphQLName()
                    {
                        Value = "scalarIntVariable"
                    }
                }
            });

            return definitions;
        }
    }
}