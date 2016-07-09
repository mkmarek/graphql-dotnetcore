namespace GraphQLCore.GraphiQLExample.Schema
{
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using Type;

    public class Query : GraphQLObjectType
    {
        public Query() : base("Query", "")
        {
            this.Field("hero", (Episode episode) => new List<ICharacter>() {
                new Human()
                {
                    Name = "Darth Vader",
                    SecretBackstory = "Luke's father"
                },
                new Droid()
                {
                    Name = "Jar Jar Bings",
                    PrimaryFunction = "Feakingly annoying"
                }
            });
            this.Field("human", (string id) => new Human());
            this.Field("droid", (string id) => new Droid());
            this.Field("test", (int[] id) => id.Sum());
        }
    }
}