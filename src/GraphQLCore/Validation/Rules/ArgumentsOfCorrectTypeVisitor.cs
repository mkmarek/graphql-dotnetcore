namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class ArgumentsOfCorrectTypeVisitor : ValidationASTVisitor
    {
        public ArgumentsOfCorrectTypeVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            var argumentType = this.GetLastArgumentType(argument);
            var astValue = argument.Value;

            var errors = this.LiteralValueValidator.IsValid(argumentType, astValue);

            if (errors.Any())
                this.Errors.Add(this.ComposeErrorMessage(argument, astValue, errors));

            return base.EndVisitArgument(argument);
        }

        private GraphQLException ComposeErrorMessage(GraphQLArgument argument, GraphQLValue astValue, IEnumerable<GraphQLException> innerExceptions)
        {
            var innerMessage = string.Join("\n", innerExceptions.Select(e => e.Message));
            var exceptionMessage = $"Argument \"{argument.Name.Value}\" has invalid value {astValue}.\n{innerMessage}";

            return new GraphQLException(exceptionMessage);
        }
    }
}