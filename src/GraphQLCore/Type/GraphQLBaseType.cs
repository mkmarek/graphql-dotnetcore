namespace GraphQLCore.Type
{
    using Introspection;
    using Translation;

    public abstract class GraphQLBaseType
    {
        public GraphQLBaseType(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Description { get; protected set; }
        public string Name { get; protected set; }

        public abstract IntrospectedType Introspect(ISchemaRepository schemaRepository);

        public override string ToString()
        {
            return this.Name;
        }

        public abstract bool IsLeafType { get; }
    }
}