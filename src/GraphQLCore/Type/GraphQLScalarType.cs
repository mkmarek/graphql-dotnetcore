namespace GraphQLCore.Type
{
    public abstract class GraphQLScalarType
    {
        public GraphQLScalarType(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public string Description { get; protected set; }
        public string Name { get; protected set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}