namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class GraphQLHumanObject : GraphQLObjectType<Human>
    {
        public GraphQLHumanObject()
        : base("Human", "A humanoid creature in the Star Wars universe.")
        {
            this.Field("id", e => e.Id).WithDescription("The id of the human.");
            this.Field("name", e => e.Name).WithDescription("The name of the human.");
            this.Field("friends", e => e.Friends).WithDescription("The friends of the human, or an empty list if they have none.");
            this.Field("appearsIn", e => e.AppearsIn).WithDescription("Which movies they appear in.");
            this.Field("secretBackstory", e => e.SecretBackstory).WithDescription("Where are they from and how they came to be who they are.");
            this.Field("homePlanet", e => e.HomePlanet).WithDescription("The home planet of the human, or null if unknown.");
        }
    }
}