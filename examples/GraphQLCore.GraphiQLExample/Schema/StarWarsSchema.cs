namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;

    public class StarWarsSchema : GraphQLSchema
    {
        public StarWarsSchema()
        {
            var rootQuery = new Query();
            var rootMutation = new Mutation();

            this.AddKnownType(new GraphQLCharacterInterface());
            this.AddKnownType(new GraphQLHumanObject());
            this.AddKnownType(new GraphQLDroidObject());
            this.AddKnownType(new GraphQLEpisodeEnum());
            this.AddKnownType(new GraphQLDroidInputObject());
            this.AddKnownType(rootQuery);
            this.AddKnownType(rootMutation);

            this.Query(rootQuery);
            this.Mutation(rootMutation);
        }
    }
}