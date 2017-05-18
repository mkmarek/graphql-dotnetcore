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

    public class KnownArgumentNamesVisitor : ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;

        public KnownArgumentNamesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var argumentName = argument.Name.Value;
            var directive = this.GetDirective();

            if (directive != null)
            {
                var directiveDefinition = this.schemaRepository.GetDirective(directive.Name.Value);

                this.CheckDirectiveArgument(directiveDefinition, argumentName);
            }
            else
            {
                var fieldDefinition = this.GetLastField();

                if (fieldDefinition != null)
                    this.CheckFieldArgument(fieldDefinition, argumentName);
            }

            return base.BeginVisitArgument(argument);
        }

        private void CheckDirectiveArgument(GraphQLDirectiveType directiveType, string argumentName)
        {
            var arguments = directiveType.GetArguments();

            if (arguments.Any(e => e.Name == argumentName))
                return;

            var errorMessage = this.ComposeUnknownDirectiveArgumentMessage(
                argumentName,
                directiveType.Name,
                StringUtils.SuggestionList(argumentName, arguments.Select(e => e.Name)));

            this.Errors.Add(new GraphQLException(errorMessage));
        }

        private void CheckFieldArgument(GraphQLFieldInfo fieldInfo, string argumentName)
        {
            if (!fieldInfo.Arguments.ContainsKey(argumentName))
            {
                var parentType = this.GetParentType();

                var errorMessage = this.ComposeUnknownArgumentMessage(
                    argumentName,
                    fieldInfo.Name,
                    parentType,
                    StringUtils.SuggestionList(argumentName, fieldInfo.Arguments.Select(e => e.Value.Name)));

                this.Errors.Add(new GraphQLException(errorMessage));
            }
        }

        private string ComposeUnknownArgumentMessage(string argName, string fieldName, GraphQLBaseType type, IEnumerable<string> suggestedArgs)
        {
            var message = $"Unknown argument \"{argName}\" on field \"{fieldName}\" of type \"{type}\".";

            if (suggestedArgs.Any())
                message += $" Did you mean {StringUtils.QuotedOrList(suggestedArgs)}?";

            return message;
        }

        private string ComposeUnknownDirectiveArgumentMessage(string argName, string directiveName, IEnumerable<string> suggestedArgs)
        {
            var message = $"Unknown argument \"{argName}\" on directive \"{directiveName}\".";

            if (suggestedArgs.Any())
                message += $" Did you mean {StringUtils.QuotedOrList(suggestedArgs)}?";

            return message;
        }
    }
}