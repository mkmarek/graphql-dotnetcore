namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class GraphQLDroidObject : GraphQLObjectType<Droid>
    {
        public GraphQLDroidObject()
                : base("Droid", "A character in the Star Wars Trilogy")
        {
            this.Field("id", e => e.Id);
            this.Field("name", e => e.Name);
            this.Field("friends", e => e.Friends);
            this.Field("appearsIn", e => e.AppearsIn);
            this.Field("primaryFunction", e => e.PrimaryFunction);
        }
    }
}