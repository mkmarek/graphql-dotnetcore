using GraphQLCore.Type.Introspection;
using GraphQLCore.Type.Translation;

namespace GraphQLCore.Type
{
    public abstract class GraphQLBaseType
    {
        public GraphQLBaseType(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Description { get; protected set; }
        public string Name { get; protected set; }

        public abstract IntrospectedType Introspect(ISchemaObserver schemaObserver);

        public override string ToString()
        {
            return this.Name;
        }
    }
}