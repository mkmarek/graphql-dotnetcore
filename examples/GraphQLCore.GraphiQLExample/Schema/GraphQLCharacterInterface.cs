namespace GraphQLCore.GraphiQLExample.Schema
{
    using Type;
    using Models;

    public class GraphQLCharacterInterface : GraphQLInterfaceType<ICharacter>
    {
        public GraphQLCharacterInterface(GraphQLSchema schema) 
            : base("Character", "A character in the Star Wars Trilogy", schema)
        {
            this.Field("id", e => e.Id);
            this.Field("name", e => e.Name);
            this.Field("friends", e => e.Friends);
            this.Field("appearsIn", e => e.AppearsIn);
            this.Field("secretBackstory", e => e.SecretBackstory);
        }
    }
}
