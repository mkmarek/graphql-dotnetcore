namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Directives;
    using Type.Translation;

    public class KnownDirectivesVisitor : ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;
        private Stack<ASTNode> ancestorStack;

        public KnownDirectivesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.ancestorStack = new Stack<ASTNode>();
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override ASTNode BeginVisitNode(ASTNode node)
        {
            this.ancestorStack.Push(node);
            node = base.BeginVisitNode(node);
            this.ancestorStack.Pop();

            return node;
        }

        public override GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            var directiveDefinition = this.schemaRepository.GetDirective(directive.Name.Value);

            if (directiveDefinition == null)
            {
                var errorMessage = this.ComposeUnknownDirectiveMessage(directive.Name.Value);

                this.Errors.Add(new GraphQLException(errorMessage, new[] { directive }));
            }
            else
            {
                var candidateLocation = this.GetDirectiveLocationForAstPath();

                if (!directiveDefinition.Locations.Contains(candidateLocation))
                {
                    var errorMessage = this.ComposeMisplacedDirectiveMessage(directive.Name.Value, candidateLocation.ToString());
                    this.Errors.Add(new GraphQLException(errorMessage, new[] { directive }));
                }
            }

            return base.BeginVisitDirective(directive);
        }

        private DirectiveLocation GetDirectiveLocationForAstPath()
        {
            var appliedTo = this.GetLastNode();

            switch (appliedTo.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    var definition = (GraphQLOperationDefinition)appliedTo;

                    switch (definition.Operation)
                    {
                        case OperationType.Query:
                            return DirectiveLocation.QUERY;
                        case OperationType.Mutation:
                            return DirectiveLocation.MUTATION;
                        case OperationType.Subscription:
                            return DirectiveLocation.SUBSCRIPTION;
                    }

                    throw new NotImplementedException();
                case ASTNodeKind.Field:
                    return DirectiveLocation.FIELD;
                case ASTNodeKind.FragmentSpread:
                    return DirectiveLocation.FRAGMENT_SPREAD;
                case ASTNodeKind.InlineFragment:
                    return DirectiveLocation.INLINE_FRAGMENT;
                case ASTNodeKind.FragmentDefinition:
                    return DirectiveLocation.FRAGMENT_DEFINITION;
                case ASTNodeKind.SchemaDefinition:
                    return DirectiveLocation.SCHEMA;
                case ASTNodeKind.ScalarTypeDefinition:
                    return DirectiveLocation.SCALAR;
                case ASTNodeKind.ObjectTypeDefinition:
                    return DirectiveLocation.OBJECT;
                case ASTNodeKind.FieldDefinition:
                    return DirectiveLocation.FIELD_DEFINITION;
                case ASTNodeKind.InterfaceTypeDefinition:
                    return DirectiveLocation.INTERFACE;
                case ASTNodeKind.UnionTypeDefinition:
                    return DirectiveLocation.UNION;
                case ASTNodeKind.EnumTypeDefinition:
                    return DirectiveLocation.ENUM;
                case ASTNodeKind.EnumValueDefinition:
                    return DirectiveLocation.ENUM_VALUE;
                case ASTNodeKind.InputObjectTypeDefinition:
                    return DirectiveLocation.INPUT_OBJECT;
                case ASTNodeKind.InputValueDefinition:
                    if (this.GetParentNode().Kind == ASTNodeKind.InputObjectTypeDefinition)
                        return DirectiveLocation.INPUT_FIELD_DEFINITION;
                    return DirectiveLocation.ARGUMENT_DEFINITION;
                default:
                    throw new NotImplementedException();
            }
        }

        private ASTNode GetLastNode()
        {
            return this.ancestorStack.Skip(1).FirstOrDefault();
        }

        private ASTNode GetParentNode()
        {
            return this.ancestorStack.Skip(2).FirstOrDefault();
        }

        private string ComposeUnknownDirectiveMessage(string directiveName)
        {
            return $"Unknown directive \"{directiveName}\".";
        }

        private string ComposeMisplacedDirectiveMessage(string directiveName, string location)
        {
            return $"Directive \"{directiveName}\" may not be used on {location}.";
        }
    }
}
