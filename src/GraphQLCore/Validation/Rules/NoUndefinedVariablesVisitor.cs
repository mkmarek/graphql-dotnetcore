namespace GraphQLCore.Validation.Rules
{
    using Abstract;
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;
    using Utils;

    public class NoUndefinedVariablesVisitor : VariableValidationVisitor
    {
        private GraphQLDocument document;
        private List<string> variableDefinitions;

        public NoUndefinedVariablesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            this.variableDefinitions = new List<string>();

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            var name = node.Variable.Name.Value;

            if (!this.variableDefinitions.Contains(name))
                this.variableDefinitions.Add(node.Variable.Name.Value);

            return base.BeginVisitVariableDefinition(node);
        }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            var variableUsages = VariableUsagesProvider.Get(definition, this.document, this.Schema);

            foreach (var usage in variableUsages)
                this.VerifyUsage(definition, usage, definition.Name?.Value);

            return base.EndVisitOperationDefinition(definition);
        }

        public override void Visit(GraphQLDocument ast)
        {
            this.document = ast;

            base.Visit(ast);
        }

        private void VerifyUsage(GraphQLOperationDefinition operation, VariableUsage usage, string opName)
        {
            var variableName = usage.Variable.Name.Value;

            if (!this.variableDefinitions.Contains(variableName))
                this.Errors.Add(new GraphQLException(this.ComposeUndefinedVarMessage(variableName, opName),
                    new ASTNode[] { usage.Variable, operation }));
        }

        private string ComposeUndefinedVarMessage(string varName, string opName)
        {
            if (!string.IsNullOrEmpty(opName))
                return $"Variable \"${varName}\" is not defined by operation \"{opName}\".";
            return $"Variable \"${varName}\" is not defined.";
        }
    }
}
