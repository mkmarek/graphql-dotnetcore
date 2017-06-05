namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language;
    using System.Collections.Generic;
    using Type;
    using Language.AST;
    using System.Reflection;

    public class UniqueDirectivesPerLocationVisitor : GraphQLAstVisitor
    {
        public UniqueDirectivesPerLocationVisitor()
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        private string DuplicateDirectiveMessage(string directiveName)
        {
            return $"The directive {directiveName} can only be used once at this location.";
        }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            var appliedDirectives = new List<string>();
            if (node is IWithDirectives)
            {
                var nodeWithDirectives = node as IWithDirectives;

                foreach (var directive in nodeWithDirectives.Directives)
                {
                    var directiveName = directive.Name.Value;

                    if (appliedDirectives.Contains(directiveName))
                    {
                        this.Errors.Add(new GraphQLException(this.DuplicateDirectiveMessage(directiveName)));
                    }
                    else
                    {
                        appliedDirectives.Add(directiveName);
                    }
                }
            }

            return base.BeginVisitNode(node);
        }
    }
}