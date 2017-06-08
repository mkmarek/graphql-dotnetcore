namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using Type;
    using Type.Translation;

    public class FragmentsOnCompositeTypesVisitor : ValidationASTVisitor
    {
        private ISchemaRepository schemaRepository;

        public FragmentsOnCompositeTypesVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.schemaRepository = schema.SchemaRepository;
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLInlineFragment BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            if (inlineFragment.TypeCondition != null)
            {
                var type = this.schemaRepository.GetSchemaInputTypeByName(inlineFragment.TypeCondition.Name.Value);

                if (type != null)
                {
                    var errorMessage = this.ComposeInlineFragmentOnNonCompositeErrorMessage(type);
                    this.Errors.Add(new GraphQLException(errorMessage, new[] { inlineFragment.TypeCondition }));
                }
            }

            return base.BeginVisitInlineFragment(inlineFragment);
        }

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            var type = this.schemaRepository.GetSchemaInputTypeByName(node.TypeCondition.Name.Value);

            if (type != null)
            {
                var errorMessage = this.ComposeFragmentOnNonCompositeErrorMessage(node.Name.Value, type);
                this.Errors.Add(new GraphQLException(errorMessage, new[] { node.TypeCondition }));
            }

            return base.BeginVisitFragmentDefinition(node);
        }

        private string ComposeInlineFragmentOnNonCompositeErrorMessage(GraphQLBaseType type)
        {
            return $"Fragment cannot condition on non composite type \"{type}\".";
        }

        private string ComposeFragmentOnNonCompositeErrorMessage(string fragmentName, GraphQLBaseType type)
        {
            return $"Fragment \"{fragmentName}\" cannot condition on non composite type \"{type}\".";
        }
    }
}
