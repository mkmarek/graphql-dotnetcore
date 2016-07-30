namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueOperationNamesVisitor : ValidationASTVisitor
    {
        private Dictionary<string, bool> knownOperationNames = new Dictionary<string, bool>();

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
                    this.ReportOperationNameError(operationName);
                else
                    this.knownOperationNames.Add(operationName, true);
            }

            return definition;
        }

        private void ReportOperationNameError(string operationName)
        {
            this.Errors.Add(new GraphQLException($"There can only be one operation named \"{operationName}\"."));
        }
    }
}