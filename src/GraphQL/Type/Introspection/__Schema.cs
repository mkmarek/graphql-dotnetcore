using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Type.Introspection
{
    public class __Schema : GraphQLObjectType
    {
        private GraphQLSchema schema;

        public __Schema(GraphQLSchema schema) : base("__Schema", "A GraphQL Schema defines the capabilities of a GraphQL server. It " +
            "exposes all available types and directives on the server, as well as " +
            "the entry points for query, mutation, and subscription operations.", schema)
        {
            this.schema = schema;

            this.Field("types", () => schema.SchemaTypes);
        }
    }
}
