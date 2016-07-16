# Input object definition

Input object definitions are for defining the form of object that
you can send into the query either through variable or inline.
This definition needs to be separated from an output object definition
due to the fact that input object definitions don't support any
resolvers or nested types.

## Example
### Definition

```csharp
public class GraphQLDroidInputObject : GraphQLInputObjectType<Droid>
{
    public GraphQLDroidInputObject()
            : base("InputDroid", "Input object for a character in the Star Wars Trilogy")
    {
        this.Field("id", e => e.Id);
        this.Field("name", e => e.Name);
        this.Field("appearsIn", e => e.AppearsIn);
        this.Field("primaryFunction", e => e.PrimaryFunction);
    }
}

public class Mutation : GraphQLObjectType
{
    private CharacterService service = new CharacterService();

    public Mutation() : base("Mutation", "")
    {
        this.Field("addDroid", (Droid droid) => this.CreateAndGet(droid));
    }

    private Droid CreateAndGet(Droid droid)
    {
        return service.CreateDroid(droid);
    }
}
```

### Query

```graphql
mutation {
  addDroid(droid: {
    name : "Death Star Droid",
    appearsIn: [NEWHOPE],
    primaryFunction: "Spying on Imperial officers" 
  }) {
    id,
    name,
    appearsIn,
    primaryFunction
  }
}
```

### Result

```json
{
  "addDroid": {
    "id" : "c8d97725-d750-4a70-bde2-caad69305904",
    "name" : "Death Star Droid",
    "appearsIn" : [ "NEWHOPE" ],
    "primaryFunction" : "Spying on Imperial officers"
  }
}
```

## Accessors
On input object types you can define only accessors. 
For more information about accessors have a look into the
[typed object definition section](../typed-object-definition/overview.md#accessors).