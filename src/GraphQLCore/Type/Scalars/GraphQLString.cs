using System;
using GraphQLCore.Language.AST;

namespace GraphQLCore.Type.Scalars
{
    public class GraphQLString : GraphQLScalarType
    {
        public GraphQLString(GraphQLSchema schema) : base("String",
            "The `String` scalar type represents textual data, represented as UTF-8 " +
            "character sequences. The String type is most often used by GraphQL to " +
            "represent free-form human-readable text.",
            schema)
        {
        }
    }
}
