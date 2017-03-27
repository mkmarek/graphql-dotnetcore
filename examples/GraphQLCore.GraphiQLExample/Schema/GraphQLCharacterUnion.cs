namespace GraphQLCore.GraphiQLExample.Schema
{
    using System;
    using Models;
    using Type;

    public class GraphQLCharacterUnion : GraphQLUnionType
    {
        public GraphQLCharacterUnion()
            : base("CharacterUnion", "An union of characters in the Star Wars Trilogy")
        {
            this.AddPossibleType(typeof(Droid));
            this.AddPossibleType(typeof(Human));
        }

        public override Type ResolveType(object data)
        {
            if (data is Droid)
                return typeof(Droid);
            else if (data is Human)
                return typeof(Human);

            //If null is returned then the result won't be returned
            return null;
        }
    }
}