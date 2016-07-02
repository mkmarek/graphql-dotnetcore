namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;

    public class StarWarsSchema : GraphQLSchema
    {
        public StarWarsSchema()
        {
            this.Query(new Query(this));
        }
    }
}
