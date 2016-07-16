namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class GraphQLDroidInputObject : GraphQLInputObjectType<Droid>
    {
        public GraphQLDroidInputObject()
                : base("InputDroid", "Input object for a character in the Star Wars Trilogy")
        {
            this.Field("name", e => e.Name);
            this.Field("appearsIn", e => e.AppearsIn);
            this.Field("primaryFunction", e => e.PrimaryFunction);
        }
    }
}