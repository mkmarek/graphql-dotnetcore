using System;
using GraphQLCore.Language.AST;

namespace GraphQLCore.Type.Scalars
{
    public class GraphQLInt : GraphQLScalarType
    {
        public GraphQLInt(GraphQLSchema schema) : base("Int", 
            "The `Int` scalar type represents non-fractional signed whole numeric values. Int can represent values between -(2^31) and 2^31 - 1.",
            schema)
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
