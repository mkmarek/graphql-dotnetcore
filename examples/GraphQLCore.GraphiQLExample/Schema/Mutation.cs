namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Services;
    using Type;

    public class Mutation : GraphQLObjectType
    {
        public Mutation() : base("Mutation", "")
        {
            var service = new CharacterService();

            this.Field("addDroid", (Droid droid) => service.GetDroidById("2000"));
        }
    }
}