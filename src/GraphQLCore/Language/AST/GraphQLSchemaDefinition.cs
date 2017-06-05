using System.Collections.Generic;

namespace GraphQLCore.Language.AST
{
    public class GraphQLSchemaDefinition : ASTNode, IWithDirectives
    {
        public IEnumerable<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind
        {
            get
            {
                return ASTNodeKind.SchemaDefinition;
            }
        }

        public IEnumerable<GraphQLOperationTypeDefinition> OperationTypes { get; set; }
    }
}