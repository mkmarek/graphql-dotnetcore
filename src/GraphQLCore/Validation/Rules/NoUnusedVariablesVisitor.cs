namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class NoUnusedVariablesVisitor : ValidationASTVisitor
    {
        private List<GraphQLVariableDefinition> variableDefinitions;
        private GraphQLDocument document;

        public NoUnusedVariablesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition EndVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            var variableUsages = VariableUsagesProvider.Get(definition, this.document, this.Schema);
            var operationName = definition.Name != null ? definition.Name.Value : null;

            foreach (var variableDefinition in this.variableDefinitions)
            {
                var variableName = variableDefinition.Variable.Name.Value;
                if (!variableUsages.Any(e => e.Variable.Name.Value == variableName))
                {
                    this.Errors.Add(new GraphQLException(this.GetUnusedVariablesMessage(variableName, operationName),
                        new[] { variableDefinition }));
                }
            }

            return base.EndVisitOperationDefinition(definition);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            this.variableDefinitions = new List<GraphQLVariableDefinition>();
            return base.BeginVisitOperationDefinition(definition);
        }

        public override void Visit(GraphQLDocument ast)
        {
            this.document = ast;

            base.Visit(ast);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            this.variableDefinitions.Add(node);

            return base.BeginVisitVariableDefinition(node);
        }

        private string GetUnusedVariablesMessage(string variableName, string operationName)
        {
            return string.IsNullOrWhiteSpace(operationName) ?
                $"Variable \"${variableName}\" is never used." :
                $"Variable \"${variableName}\" is never used in operation \"{operationName}\".";
        }
    }
}