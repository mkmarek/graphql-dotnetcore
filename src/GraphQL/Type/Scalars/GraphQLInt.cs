using System;
using GraphQL.Language.AST;

namespace GraphQL.Type.Scalars
{
    public class GraphQLInt : GraphQLScalarType
    {
        private const string NAME = "Int";
        private const string DESCRIPTION = "The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.";

        public GraphQLInt() : base(NAME, DESCRIPTION)
        {
        }

        public int? GetFromAst(ASTNode value)
        {
            if (value.Kind == ASTNodeKind.IntValue)
                return ((GraphQLValue<int>)value).Value;
            return null;
        }
    }
}
