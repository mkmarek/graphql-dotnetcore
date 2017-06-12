namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Translation;
    using Utils;

    public class SingleFieldSubscriptionsVisitor: ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;

        public SingleFieldSubscriptionsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            if (definition.Operation == OperationType.Subscription)
            {
                if (definition.SelectionSet?.Selections?.Count() != 1)
                {
                    this.Errors.Add(new GraphQLException(
                      this.SingleFieldOnlyMessage(definition.Name?.Value),
                      definition.SelectionSet.Selections.Skip(1).ToArray()));
                }
            }

            return definition;
        }

        private string SingleFieldOnlyMessage(string name)
        {
            return (string.IsNullOrWhiteSpace(name) ? "Anonymous Subscription" : $"Subscription \"{name}\"") +
              " must select only one top level field.";
        }
    }
}