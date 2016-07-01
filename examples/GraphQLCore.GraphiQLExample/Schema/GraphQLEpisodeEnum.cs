namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;
    using Models;

    public class GraphQLEpisodeEnum : GraphQLEnumType<Episode>
    {
        public GraphQLEpisodeEnum(GraphQLSchema schema) : 
            base("Episode", "One of the films in the Star Wars Trilogy", schema)
        {
        }
    }
}
