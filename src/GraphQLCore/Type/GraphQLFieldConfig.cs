namespace GraphQLCore.Type
{
    using System.Collections.Generic;

    public class GraphQLFieldConfig
    {
        public IDictionary<string, GraphQLScalarType> Arguments { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public GraphQLScalarType Type { get; set; }
    }
}