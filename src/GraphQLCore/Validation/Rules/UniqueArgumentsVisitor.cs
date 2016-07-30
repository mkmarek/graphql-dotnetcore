namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueArgumentsVisitor : ValidationASTVisitor
    {
        private Dictionary<string, bool> knownArgumentNames;

        public UniqueArgumentsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.knownArgumentNames = new Dictionary<string, bool>();

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            this.knownArgumentNames = new Dictionary<string, bool>();

            return base.BeginVisitDirective(directive);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var argumentName = argument.Name.Value;

            if (this.knownArgumentNames.ContainsKey(argumentName))
                this.ReportDuplicateArgumentsError(argumentName);
            else
                this.knownArgumentNames.Add(argumentName, true);

            return base.BeginVisitArgument(argument);
        }

        private void ReportDuplicateArgumentsError(string argumentName)
        {
            this.Errors.Add(new GraphQLException($"There can be only one argument named \"{argumentName}\"."));
        }
    }
}