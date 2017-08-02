namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using Language.AST;
    using System.Collections.Generic;

    public class UniqueDirectivesPerLocationVisitor : GraphQLAstVisitor
    {
        public UniqueDirectivesPerLocationVisitor()
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            var appliedDirectives = new Dictionary<string, GraphQLDirective>();
            if (node is IWithDirectives)
            {
                var nodeWithDirectives = node as IWithDirectives;

                foreach (var directive in nodeWithDirectives.Directives)
                {
                    var directiveName = directive.Name.Value;

                    if (appliedDirectives.ContainsKey(directiveName))
                    {
                        this.Errors.Add(new GraphQLException(this.DuplicateDirectiveMessage(directiveName),
                            new[] { appliedDirectives[directiveName], directive }));
                    }
                    else
                    {
                        appliedDirectives.Add(directiveName, directive);
                    }
                }
            }

            return base.BeginVisitNode(node);
        }

        private string DuplicateDirectiveMessage(string directiveName)
        {
            return $"The directive {directiveName} can only be used once at this location.";
        }
    }
}