namespace GraphQLCore.Type
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class GraphQLComplexType : GraphQLNullableType
    {
        protected Dictionary<string, GraphQLObjectTypeFieldInfo> fields;

        public GraphQLComplexType(string name, string description) : base(name, description)
        {
            this.fields = new Dictionary<string, GraphQLObjectTypeFieldInfo>();
        }

        public bool ContainsField(string fieldName)
        {
            return this.fields.ContainsKey(fieldName);
        }

        public GraphQLObjectTypeFieldInfo GetFieldInfo(string fieldName)
        {
            return this.fields[fieldName];
        }

        public GraphQLObjectTypeFieldInfo[] GetFieldsInfo()
        {
            return this.fields.Select(e => e.Value)
                .ToArray();
        }
    }
}