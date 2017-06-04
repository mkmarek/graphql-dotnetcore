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

        public override GraphQLInlineFragment EndVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            var fragmentType = this.GetLastType();
            var parentType = this.GetParentType();

            this.ValidateInlineFragmentTypes(fragmentType, parentType);

            return base.EndVisitInlineFragment(inlineFragment);
        }

        private void ValidateInlineFragmentTypes(GraphQLBaseType fragmentType, GraphQLBaseType parentType)
        {
            if (parentType is GraphQLList)
            {
                parentType = ((GraphQLList)parentType).MemberType;

                this.ValidateInlineFragmentTypes(fragmentType, parentType);
            }

            else if (fragmentType != null &&
                parentType != null &&
                !this.DoTypesOverlap(fragmentType, parentType))
            {
                this.Errors.Add(
                    new GraphQLException(
                        this.GetIncompatibleTypeInAnonymousFragmentMessage(fragmentType, parentType)));
            }
        }

        public override GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            if (this.Fragments.ContainsKey(fragmentSpread.Name.Value))
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
                .Interfaces?.Any(e => e.Name == parentType.Name) ?? false;

            var fragmentTypeImplementsParent = parentType
                .Introspect(this.SchemaRepository)
                .Interfaces?.Any(e => e.Name == fragmentType.Name) ?? false;

            var fragmentTypeIsWithinPossibleTypes = parentType
                .Introspect(this.SchemaRepository)
                .PossibleTypes?.Any(e => e.Name == fragmentType.Name) ?? false;

            return parentImplementsFragmentType || fragmentTypeImplementsParent || fragmentTypeIsWithinPossibleTypes;
        }

        private GraphQLBaseType GetFragmentType(GraphQLFragmentDefinition fragmentDefinition)
        {
            return this.SchemaRepository.GetSchemaOutputTypeByName(fragmentDefinition.TypeCondition.Name.Value);
        }
    }
}