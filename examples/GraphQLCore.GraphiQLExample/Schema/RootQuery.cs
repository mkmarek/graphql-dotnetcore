namespace GraphQLCore.GraphiQLExample.Schema
{
    using GraphQLCore.Type;

    public class RootQuery : GraphQLObjectType
    {
        public RootQuery(GraphQLSchema schema) : base("Root", "", schema)
        {
            this.Field("test", () => "This is a test");
        }
    }
}
