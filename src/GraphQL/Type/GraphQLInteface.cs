using System.Collections;
using System.Collections.Generic;

namespace GraphQL.Type
{
    public class GraphQLInterface : GraphQLScalarType
    {
        private IList<GraphQLInterfaceType> Types;

        public GraphQLInterface(string name, string description, GraphQLSchema schema) : base(name, description, schema)
        {
            this.Types = new List<GraphQLInterfaceType>();
        }

        public void AddField<T>(string name)
        {
            this.Types.Add(new GraphQLInterfaceType() { Name = name, Type = typeof(T) });
        }

        private class GraphQLInterfaceType
        {
            public System.Type Type { get; set; }
            public string Name { get; set; }
        }
    }
}
