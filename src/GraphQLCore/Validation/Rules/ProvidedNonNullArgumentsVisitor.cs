namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;
    using Type.Translation;

    public class ProvidedNonNullArgumentsVisitor : ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;

        public ProvidedNonNullArgumentsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            var field = this.GetLastField();

            if (field != null)
            {
                var providedArguments = selection.Arguments;

                foreach (var argument in field.Arguments)
                {
                    this.ValidateArgument(selection, argument.Value, field, providedArguments);
                }
            }

            return base.EndVisitFieldSelection(selection);
        }

        public override GraphQLDirective EndVisitDirective(GraphQLDirective directive)
        {
            var directiveDefinition = this.schemaRepository.GetDirective(directive.Name.Value);

            if (directiveDefinition != null)
            {
                var definedArguments = directiveDefinition.GetArguments();

                foreach (var argument in definedArguments)
                {
                    this.ValidateDirectiveArgument(directive, argument, directive.Arguments);
                }
            }

            return base.EndVisitDirective(directive);
        }

        private void ValidateArgument(
            GraphQLFieldSelection node,
            GraphQLObjectTypeArgumentInfo argument,
            GraphQLFieldInfo field,
            IEnumerable<GraphQLArgument> providedArguments)
        {
            var providedArgument = providedArguments.FirstOrDefault(e => e.Name.Value == argument.Name);
            var argumentType = argument.GetGraphQLType(this.SchemaRepository);

            if (providedArgument == null && argumentType is GraphQLNonNull)
            {
                this.Errors.Add(
                    new GraphQLException(
                        $"Field \"{field.Name}\" argument \"{argument.Name}\" of type \"{argumentType}\" is required but not provided.",
                        new[] { node }));
            }
        }

        private void ValidateDirectiveArgument(
            GraphQLDirective directive,
            GraphQLObjectTypeArgumentInfo argument,
            IEnumerable<GraphQLArgument> providedArguments)
        {
            var providedArgument = providedArguments.FirstOrDefault(e => e.Name.Value == argument.Name);
            var argumentType = argument.GetGraphQLType(this.SchemaRepository);

            if (providedArgument == null && argumentType is GraphQLNonNull)
            {
                this.Errors.Add(
                    new GraphQLException(
                        $"Directive \"{directive.Name.Value}\" argument \"{argument.Name}\" of type \"{argumentType}\" is required but not provided.",
                        new[] { directive }));
            }
        }
    }
}