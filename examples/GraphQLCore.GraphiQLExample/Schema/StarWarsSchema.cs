namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;

    public class StarWarsSchema : GraphQLSchema
    {
        public StarWarsSchema()
        {
            var rootQuery = new Query();
            var rootMutation = new Mutation();
            var subscriptionType = new Subscription();

            this.AddKnownType(new GraphQLCharacterUnion());
            this.AddKnownType(new GraphQLCharacterInterface());
            this.AddKnownType(new GraphQLHumanObject());
            this.AddKnownType(new GraphQLDroidObject());
            this.AddKnownType(new GraphQLEpisodeEnum());
            this.AddKnownType(new GraphQLDroidInputObject());
            this.AddKnownType(rootQuery);
            this.AddKnownType(rootMutation);
            this.AddKnownType(subscriptionType);

            this.Query(rootQuery);
            this.Mutation(rootMutation);
            this.Subscription(subscriptionType);
        }
    }
}