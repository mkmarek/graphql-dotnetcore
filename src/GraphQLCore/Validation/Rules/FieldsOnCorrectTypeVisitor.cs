namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Utils;

    public class FieldsOnCorrectTypeVisitor : ValidationASTVisitor
    {
        public FieldsOnCorrectTypeVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            var type = this.GetUnderlyingType(this.GetLastType());

            if (type != null)
            {
                var fieldName = selection.Name.Value;
                var fieldDef = this.GetField(type, fieldName);

                if (fieldDef == null)
                {
                    var suggestedTypeNames = this.GetSuggestedTypeNames(this.Schema, type, fieldName);
                    var suggestedFieldNames = suggestedTypeNames.Any()
                        ? Enumerable.Empty<string>()
                        : this.GetSuggestedFieldNames(this.Schema, type, fieldName);

                    var errorMessage = this.ComposeErrorMessage(
                        fieldName,
                        type.Name,
                        suggestedTypeNames,
                        suggestedFieldNames);

                    this.Errors.Add(new GraphQLException(errorMessage, new[] { selection }));
                }
            }

            return base.BeginVisitFieldSelection(selection);
        }

        private IEnumerable<string> GetSuggestedTypeNames(IGraphQLSchema schema, GraphQLBaseType type, string fieldName)
        {
            var introspectedType = type.Introspect(schema.SchemaRepository);
            var suggestedObjectTypes = new List<string>();
            var interfaceUsageCount = new Dictionary<string, int>();

            if (introspectedType.Value.PossibleTypes == null)
                return suggestedObjectTypes;

            foreach (var possibleType in introspectedType.Value.PossibleTypes)
            {
                if (possibleType.Value.Fields.Any(e => e.Value.Name == fieldName))
                {
                    suggestedObjectTypes.Add(possibleType.Value.Name);

                    foreach (var possibleInterface in possibleType.Value.Interfaces)
                    {
                        if (possibleInterface.Value.Fields.Any(e => e.Value.Name == fieldName))
                        {
                            if (!interfaceUsageCount.ContainsKey(possibleInterface.Value.Name))
                                interfaceUsageCount.Add(possibleInterface.Value.Name, 1);
                            else
                                interfaceUsageCount[possibleInterface.Value.Name]++;
                        }
                    }
                }
            }

            var suggestedInterfaceTypes = interfaceUsageCount.OrderByDescending(e => e.Value).Select(e => e.Key);

            return suggestedInterfaceTypes.Concat(suggestedObjectTypes);
        }

        private IEnumerable<string> GetSuggestedFieldNames(IGraphQLSchema schema, GraphQLBaseType type, string fieldName)
        {
            if (type is GraphQLObjectType || type is GraphQLInterfaceType)
            {
                var introspectedType = type.Introspect(schema.SchemaRepository);
                var possibleFieldNames = introspectedType.Value.Fields.Select(e => e.Value.Name.Value);

                return StringUtils.SuggestionList(fieldName, possibleFieldNames);
            }

            return Enumerable.Empty<string>();
        }

        private string ComposeErrorMessage(string fieldName, string type, IEnumerable<string> suggestedTypeNames, IEnumerable<string> suggestedFieldNames)
        {
            var message = $"Cannot query field \"{fieldName}\" on type \"{type}\".";

            if (suggestedTypeNames.Any())
                message += $" Did you mean to use an inline fragment on {StringUtils.QuotedOrList(suggestedTypeNames)}?";
            else if (suggestedFieldNames.Any())
                message += $" Did you mean {StringUtils.QuotedOrList(suggestedFieldNames)}?";

            return message;
        }
    }
}
