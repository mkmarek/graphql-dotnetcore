namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class Query : GraphQLObjectType
    {
        public GraphQLCharacterInterface Character { get; private set; }
        public GraphQLHumanObject Human { get; private set; }
        public GraphQLDroidObject Droid { get; private set; }

        public GraphQLEpisodeEnum Episode { get; private set; }

        public Query(GraphQLSchema schema) : base("Query", "", schema)
        {
            this.Character = new GraphQLCharacterInterface(schema);
            this.Human = new GraphQLHumanObject(schema);
            this.Droid = new GraphQLDroidObject(schema);
            this.Episode = new GraphQLEpisodeEnum(schema);

            this.Field("hero", (Episode episode) => (ICharacter)new Human());
            this.Field("human", (string id) => new Human());
            this.Field("droid", (string id) => new Droid());
        }
    }
}