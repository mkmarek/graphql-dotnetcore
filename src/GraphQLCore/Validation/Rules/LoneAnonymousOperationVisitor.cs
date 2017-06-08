namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class LoneAnonymousOperationVisitor : ValidationASTVisitor
    {
        private int operationCount;

        public LoneAnonymousOperationVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override void Visit(GraphQLDocument document)
        {
            this.operationCount = document.Definitions
                .Where(e => e.Kind == ASTNodeKind.OperationDefinition)
                .Count();

            base.Visit(document);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            if (definition.Name == null && this.operationCount > 1)
            {
                this.Errors.Add(new GraphQLException("This anonymous operation must be the only defined operation.", new[] { definition }));
            }

            return definition;
        }
    }
}