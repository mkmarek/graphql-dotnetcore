namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Services;
    using Type;

    public class Mutation : GraphQLObjectType
    {
        private CharacterService service = new CharacterService();

        public Mutation() : base("Mutation", "")
        {
            this.Field("addDroid", (Droid droid) => this.CreateAndGet(droid));
        }

        private Droid CreateAndGet(Droid droid)
        {
            return service.GetDroidById(droid.Id);
        }
    }
}