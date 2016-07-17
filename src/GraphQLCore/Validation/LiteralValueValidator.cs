namespace GraphQLCore.Validation
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class LiteralValueValidator
    {
        public LiteralValueValidator()
        {
        }

        internal IEnumerable<GraphQLException> IsValid(GraphQLBaseType type, GraphQLValue astValue)
        {
            if (type is GraphQLList)
                return this.ValidateListType(type, astValue);

            if (type is GraphQLInputType)
                return ValidateInputType(type, astValue);

            return new GraphQLException[] { };
        }

        private static IEnumerable<GraphQLException> ValidateInputType(GraphQLBaseType type, GraphQLValue astValue)
        {
            var value = ((GraphQLInputType)type).GetFromAst(astValue);

            if (value == null)
            {
                return new GraphQLException[]
                {
                    new GraphQLException($"Expected type \"{type.Name}\", found {astValue}")
                };
            }

            return new GraphQLException[] { };
        }

        private IEnumerable<GraphQLException> ValidateListMembers(GraphQLBaseType itemType, GraphQLListValue astValue)
        {
            var values = astValue.Values;

            for (int i = 0; i < values.Count(); i++)
            {
                foreach (var error in this.IsValid(itemType, values.ElementAt(i)))
                    yield return new GraphQLException($"In element #{i}: {error.Message}");
            }
        }

        private IEnumerable<GraphQLException> ValidateListType(GraphQLBaseType type, GraphQLValue astValue)
        {
            var itemType = ((GraphQLList)type).MemberType;

            if (astValue.Kind == ASTNodeKind.ListValue)
                return this.ValidateListMembers(itemType, (GraphQLListValue)astValue);

            return this.IsValid(itemType, astValue);
        }
    }
}