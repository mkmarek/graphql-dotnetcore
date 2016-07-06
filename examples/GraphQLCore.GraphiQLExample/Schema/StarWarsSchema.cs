namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;

    public class StarWarsSchema : GraphQLSchema
    {
        public StarWarsSchema()
        {
            var rootQuery = new Query();

            this.AddKnownType(new GraphQLCharacterInterface());
            this.AddKnownType(new GraphQLHumanObject());
            this.AddKnownType(new GraphQLDroidObject());
            this.AddKnownType(new GraphQLEpisodeEnum());
            this.AddKnownType(rootQuery);

            this.Query(rootQuery);
        }
    }
}