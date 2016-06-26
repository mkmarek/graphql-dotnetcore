namespace GraphQLCore.Type
{
    public abstract class GraphQLScalarType
    {
        public GraphQLScalarType(string name, string description, GraphQLSchema schema)
        {
            this.Name = name;
            this.Description = description;

            if (schema != null)
                schema.RegisterType(this);
        }

        public string Description { get; protected set; }
        public string Name { get; protected set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}