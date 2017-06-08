namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueOperationNamesVisitor : ValidationASTVisitor
    {
        private Dictionary<string, GraphQLName> knownOperationNames = new Dictionary<string, GraphQLName>();

        public UniqueOperationNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            var operationName = definition?.Name?.Value;

            if (!string.IsNullOrWhiteSpace(operationName))
            {
                if (this.knownOperationNames.ContainsKey(operationName))
                    this.ReportOperationNameError(operationName, new[] { this.knownOperationNames[operationName], definition.Name });
                else
                    this.knownOperationNames.Add(operationName, definition.Name);
            }

            return definition;
        }

        private void ReportOperationNameError(string operationName, IEnumerable<ASTNode> nodes)
        {
            this.Errors.Add(new GraphQLException($"There can only be one operation named \"{operationName}\".", nodes));
        }
    }
}