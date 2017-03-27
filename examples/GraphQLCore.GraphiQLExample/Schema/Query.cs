namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Services;
    using Type;

    public class Query : GraphQLObjectType
    {
        public Query() : base("Query", "")
        {
            var service = new CharacterService();

            this.Field("hero", (Episode episode) => service.List(episode));
            this.Field("human", (string id) => service.GetHumanById(id));
            this.Field("droid", (string id) => service.GetDroidById(id));
            this.Field("characterUnion",
                (string id) => (service.GetDroidById(id) as object) ?? (service.GetHumanById(id) as object))
                .ResolveWithUnion<GraphQLCharacterUnion>();
        }
    }
}