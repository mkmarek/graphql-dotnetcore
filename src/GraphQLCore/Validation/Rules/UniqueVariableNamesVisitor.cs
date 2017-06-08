namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueVariableNamesVisitor : ValidationASTVisitor
    {
        private Dictionary<string, GraphQLName> knownVariableNames;

        public UniqueVariableNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            this.knownVariableNames = new Dictionary<string, GraphQLName>();

            return base.BeginVisitOperationDefinition(definition);
        }

        public override GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            var variableName = node.Variable.Name.Value;

            if (this.knownVariableNames.ContainsKey(variableName))
                this.ReportVariableNameError(variableName,
                    new[] { this.knownVariableNames[variableName], node.Variable.Name });
            else
                this.knownVariableNames.Add(variableName, node.Variable.Name);

            return base.BeginVisitVariableDefinition(node);
        }

        private void ReportVariableNameError(string variableName, IEnumerable<ASTNode> nodes)
        {
            this.Errors.Add(new GraphQLException($"There can be only one variable named \"{variableName}\".", nodes));
        }
    }
}