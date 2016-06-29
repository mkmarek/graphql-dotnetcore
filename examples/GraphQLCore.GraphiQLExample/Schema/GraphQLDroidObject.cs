namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;
    using Models;

    public class GraphQLDroidObject : GraphQLObjectType<Droid>
    {
        public GraphQLDroidObject(GraphQLSchema schema)
                : base("Droid", "A character in the Star Wars Trilogy", schema)
            {
            this.Field("id", e => e.Id);
            this.Field("name", e => e.Name);
            this.Field("friends", e => e.Friends);
            this.Field("appearsIn", e => e.AppearsIn);
            this.Field("secretBackstory", e => e.SecretBackstory);
            this.Field("primaryFunction", e => e.PrimaryFunction);
        }
    }
}
