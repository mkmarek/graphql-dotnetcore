using System.Collections.Generic;

namespace GraphQLCore.Language.AST
{
    public class GraphQLFieldSelection : ASTNode, IWithDirectives
    {
        public GraphQLName Alias { get; set; }

        public IEnumerable<GraphQLArgument> Arguments { get; set; }

        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.Field;
            }
        }

        public string GetPathName()
        {
            return this.Alias?.Value ?? this.Name.Value;
        }

        public GraphQLName Name { get; set; }
        public GraphQLSelectionSet SelectionSet { get; set; }
    }
}