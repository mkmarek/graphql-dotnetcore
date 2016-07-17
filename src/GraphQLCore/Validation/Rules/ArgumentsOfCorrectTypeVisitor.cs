namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
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
            var argumentType = this.GetArgumentDefinition();
            var astValue = argument.Value;

            var errors = this.LiteralValueValidator.IsValid(argumentType, astValue);

            foreach (var error in errors)
            {
                this.Errors.Add(new GraphQLException(
                    $"Argument \"{argument.Name.Value}\" has invalid value {astValue} {error.Message}"));
            }

            return base.EndVisitArgument(argument);
        }
    }
}