using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Type
{
    public abstract class GraphQLScalarType
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public GraphQLScalarType(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
