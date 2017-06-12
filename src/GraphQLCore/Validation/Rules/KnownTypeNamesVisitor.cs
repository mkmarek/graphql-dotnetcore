namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Translation;
    using Utils;

    public class KnownTypeNamesVisitor : GraphQLAstVisitor
    {
        private ISchemaRepository schemaRepository;

        public KnownTypeNamesVisitor(IGraphQLSchema schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLNamedType BeginVisitNamedType(GraphQLNamedType namedType)
        {
            var typeName = namedType.Name.Value;
            var type = this.GetTypeFromSchema(typeName);

            if (type == null)
            {
                this.Errors.Add(
                    new GraphQLException(this.ComposeErrorMessage(typeName), new[] { namedType }));
            }

            return base.BeginVisitNamedType(namedType);
        }

        private string ComposeErrorMessage(string typeName)
        {
            var schemaTypeNames = this.schemaRepository.GetInputKnownTypes()
                .Select(e => e.Name)
                .Union(this.schemaRepository.GetOutputKnownTypes()
                .Select(e => e.Name))
                .ToArray();

            var suggestedTypes = StringUtils.SuggestionList(typeName, schemaTypeNames);

            return $"Unknown type \"{typeName}\"." +
                (suggestedTypes.Any()
                    ? $" Did you mean {StringUtils.QuotedOrList(suggestedTypes)}?"
                    : string.Empty);
        }

        private GraphQLBaseType GetTypeFromSchema(string typeName)
        {
            return this.schemaRepository.GetSchemaInputTypeByName(typeName) ??
                this.schemaRepository.GetSchemaOutputTypeByName(typeName);
        }
    }
}