namespace GraphQLCore.Validation.Rules
{
    using Abstract;
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class VariablesAreInputTypesVisitor : VariableValidationVisitor
    {
        public VariablesAreInputTypesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition variableDefinition)
        {
            var variableName = variableDefinition.Variable.Name.Value;
            var inputType = this.GetOutputType(variableDefinition.Type);

            if (inputType != null)
                this.Errors.Add(new GraphQLException($"Variable \"${variableName}\" cannot be non-input type \"{variableDefinition.Type}\".",
                    new[] { variableDefinition.Type }));

            return variableDefinition;
        }
    }
}