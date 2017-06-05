namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;

    public class ProvidedNonNullArgumentsVisitor : ValidationASTVisitor
    {
        public ProvidedNonNullArgumentsVisitor(IGraphQLSchema schema) : base(schema)
        {
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
                    this.ValidateArgument(argument.Value, field, providedArguments);
                }
            }

            return base.EndVisitFieldSelection(selection);
        }

        private void ValidateArgument(
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
                        $"Field \"{field.Name}\" argument \"{argument.Name}\" of type \"{argumentType}\" is required but not provided."));
            }
        }
    }
}