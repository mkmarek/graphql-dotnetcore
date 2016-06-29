namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;
    using Models;

    public class GraphQLEpisodeEnum : GraphQLEnumType
    {
        public GraphQLEpisodeEnum(GraphQLSchema schema) : 
            base("Episode", "One of the films in the Star Wars Trilogy", typeof(Episode), schema)
        {
        }
    }
}
