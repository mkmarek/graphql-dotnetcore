namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class PossibleFragmentSpreadsVisitor : ValidationASTVisitor
    {
        public PossibleFragmentSpreadsVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLInlineFragment BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            var fragmentType = this.GetFragmentType(inlineFragment);
            var parentType = this.GetLastType();

            if (fragmentType != null &&
                parentType != null &&
                !this.DoTypesOverlap(fragmentType, parentType))
            {
                this.Errors.Add(
                    new GraphQLException(
                        this.GetIncompatibleTypeInAnonymousFragmentMessage(fragmentType, parentType)));
            }

            return base.BeginVisitInlineFragment(inlineFragment);
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            var fragmentDefinition = this.Fragments[fragmentSpread.Name.Value];
            var fragmentType = this.GetFragmentType(fragmentDefinition);
            var parentType = this.GetLastType();

            if (fragmentType != null &&
                parentType != null &&
                !this.DoTypesOverlap(fragmentType, parentType))
            {
                this.Errors.Add(
                    new GraphQLException(
                        this.GetIncompatibleTypeInFragmentMessage(
                            fragmentType, parentType, fragmentSpread.Name.Value)));
            }

            return base.BeginVisitFragmentSpread(fragmentSpread);
        }

        private string GetIncompatibleTypeInAnonymousFragmentMessage(
            GraphQLBaseType fragmentType, GraphQLBaseType parentType)
        {
            return "Fragment cannot be spread here as objects of " +
                   $"type \"{parentType}\" can never be of type \"{fragmentType}\".";
        }

        private string GetIncompatibleTypeInFragmentMessage(
            GraphQLBaseType fragmentType, GraphQLBaseType parentType, string fragmentName)
        {
            return $"Fragment {fragmentName} cannot be spread here as objects of " +
                   $"type \"{parentType}\" can never be of type \"{fragmentType}\".";
        }

        private bool DoTypesOverlap(GraphQLBaseType fragmentType, GraphQLBaseType parentType)
        {
            if (fragmentType == parentType)
                return true;

            var parentImplementsFragmentType = fragmentType
                .Introspect(this.SchemaRepository)
                .Interfaces.Any(e => e.Name == parentType.Name);

            var fragmentTypeImplementsParent = parentType
                .Introspect(this.SchemaRepository)
                .Interfaces.Any(e => e.Name == fragmentType.Name);

            return parentImplementsFragmentType || fragmentTypeImplementsParent;
        }

        private GraphQLBaseType GetFragmentType(GraphQLInlineFragment inlineFragment)
        {
            return this.SchemaRepository.GetSchemaOutputTypeByName(inlineFragment.TypeCondition.Name.Value);
        }

        private GraphQLBaseType GetFragmentType(GraphQLFragmentDefinition fragmentDefinition)
        {
            return this.SchemaRepository.GetSchemaOutputTypeByName(fragmentDefinition.TypeCondition.Name.Value);
        }
    }
}