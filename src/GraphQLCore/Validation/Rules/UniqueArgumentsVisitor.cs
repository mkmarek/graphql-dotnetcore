namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;

    public class UniqueArgumentsVisitor : ValidationASTVisitor
    {
        private Dictionary<string, GraphQLName> knownArgumentNames;

        public UniqueArgumentsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.knownArgumentNames = new Dictionary<string, GraphQLName>();

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            this.knownArgumentNames = new Dictionary<string, GraphQLName>();

            return base.BeginVisitDirective(directive);
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var argumentName = argument.Name.Value;

            if (this.knownArgumentNames.ContainsKey(argumentName))
                this.ReportDuplicateArgumentsError(argumentName, new[] { this.knownArgumentNames[argumentName], argument.Name });
            else
                this.knownArgumentNames.Add(argumentName, argument.Name);

            return base.BeginVisitArgument(argument);
        }

        private void ReportDuplicateArgumentsError(string argumentName, IEnumerable<ASTNode> nodes)
        {
            this.Errors.Add(new GraphQLException($"There can be only one argument named \"{argumentName}\".",
                nodes));
        }
    }
}