namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using System.Linq;
    using Type;

    public class Query : GraphQLObjectType
    {
        public Query() : base("Query", "")
        {
            this.Field("hero", (Episode episode) => (ICharacter)new Human());
            this.Field("human", (string id) => new Human());
            this.Field("droid", (string id) => new Droid());
            this.Field("test", (int[] id) => id.Sum());
        }
    }
}