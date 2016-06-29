namespace GraphQLCore.Type
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GraphQLEnumType : GraphQLObjectType
    {
        private Type enumType;

        public GraphQLEnumType(string name, string description, Type enumType, GraphQLSchema schema) : base(name, description, schema)
        {
            this.enumType = enumType;
        }

        public IEnumerable<GraphQLEnumValue> GetEnumValues()
        {
            return Enum.GetNames(this.enumType).Select(e => new GraphQLEnumValue(e, "", null));
        }

        public bool IsOfType(Type enumType)
        {
            return this.enumType == enumType;
        }
    }
}