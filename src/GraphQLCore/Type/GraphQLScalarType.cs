namespace GraphQLCore.Type
{
    public abstract class GraphQLScalarType
    {
        protected GraphQLSchema schema;

        public GraphQLScalarType(string name, string description, GraphQLSchema schema)
        {
            this.Name = name;
            this.Description = description;
            this.schema = schema;

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