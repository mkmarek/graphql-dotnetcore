using System;
using GraphQL.Language.AST;

namespace GraphQL.Type.Scalars
{
    public class GraphQLBoolean : GraphQLScalarType
    {
        public GraphQLBoolean(GraphQLSchema schema) : base("Boolean",
            "The `Boolean` scalar type represents `true` or `false`.",
            schema)
        {
        }
    }
}
