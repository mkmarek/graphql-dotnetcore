namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class GraphQLHumanObject : GraphQLObjectType<Human>
    {
        public GraphQLHumanObject()
                : base("Human", "A humanoid creature in the Star Wars universe.")
        {
            this.Field("id", e => e.Id);
            this.Field("name", e => e.Name);
            this.Field("friends", e => e.Friends);
            this.Field("appearsIn", e => e.AppearsIn);
            this.Field("secretBackstory", e => e.SecretBackstory);
            this.Field("homePlanet", e => e.HomePlanet);
        }
    }
}