namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;
    using Models;

    public class GraphQLHumanObject : GraphQLObjectType<Human>
    {
        public GraphQLHumanObject(GraphQLSchema schema)
                : base("Human", "A humanoid creature in the Star Wars universe.", schema)
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
