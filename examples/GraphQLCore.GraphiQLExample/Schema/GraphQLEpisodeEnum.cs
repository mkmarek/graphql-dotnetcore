namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using Type;

    public class GraphQLEpisodeEnum : GraphQLEnumType<Episode>
    {
        public GraphQLEpisodeEnum() :
            base("Episode", "One of the films in the Star Wars Trilogy")
        {
        }
    }
}