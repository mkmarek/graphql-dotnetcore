# Typed object definition

Typed object definitions are GraphQL definitions bound to a specific
POCO\*. These definitions can't be a root query but a root query can
reference its fields to these definitions by returning a class
object with type that's specified in a typed definition.

Translation from returned POCO\* to a specific definition is done
automatically.

## Example
### Definition

```csharp
class Model
{
    public string StringProperty { get; set; }
}

public class ModelType : GraphQLObjectType<Model>
{
	public ModelType() : base("Model", "Sample model definition")
	{
		this.Field("stringProp", e => e.StringProperty);
	}
}

public class Query : GraphQLObjectType
{
	public Query() : base("Query", "Sample root query defintion")
	{
		this.Field("model", () => this.GetModel());
	}

    private Model GetModel()
    {
        return new Model() { StringProperty = "Test" };
    }
}
```

### Query

```graphql
{
  model {
    stringProp
  }
}
```

### Result

```json
{
  "model" : {
    "stringProp" : "Test"
  }
}
```

## Accessors
Accessors are field definitions that aim to a specific property of
a given POCO\*. These definitions can't have any input arguments.
Accessors are defined by a lambda expression which contains one parameter
of the defined POCO\* type which represents the instance itself.

## Resolvers

Also in typed input object you can define additional resolvers.
For discovering how resolvers work please referer the resolvers
section in the
[untyped object definition section](../untyped-object-definition/overview.md#resolvers).

_\* POCO is a plain old CLR object._