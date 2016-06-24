using System;
using GraphQLCore.Language.AST;

namespace GraphQLCore.Type.Scalars
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
