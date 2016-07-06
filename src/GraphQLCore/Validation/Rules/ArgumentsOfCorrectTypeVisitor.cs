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

        public override IEnumerable<GraphQLArgument> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            var args = base.BeginVisitArguments(arguments);

            var unvisitedArguments = this.GetUnvisitedArguments();

            foreach (var argumentType in unvisitedArguments)
                this.Errors.AddRange(this.schema.TypeTranslator.IsValidLiteralValue(argumentType, null));

            return args;
        }

        public override GraphQLArgument EndVisitArgument(GraphQLArgument argument)
        {
            var argumentType = base.GetArgumentDefinition();
            this.Errors.AddRange(this.schema.TypeTranslator.IsValidLiteralValue(argumentType, argument.Value));

            return base.EndVisitArgument(argument);
        }
    }
}