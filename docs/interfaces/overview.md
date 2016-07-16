# Interfaces

You can define interfaces that are containing common properties for
a set of objects. These interfaces you can leverage for example
while using fragments in your query.

Interfaces can only contain accessors as they just act as a template
for typed object types;

The dependency between interfaces and their implementations is
resolved automatically from the defined POCOs.

## Example
### Definition

```csharp
interface IModel
{
    string StringProperty { get; set; }
}

class ModelWithIntProp : IModel
{
    public string StringProperty { get; set; }
    public int AdditionalIntProperty { get; set; }
}

class ModelWithBoolProp : IModel
{
    public string StringProperty { get; set; }
    public bool AdditionalBoolProperty { get; set; }
}

public class InterfaceModelType : GraphQLInterfaceType<IModel>
{
	public ModelType() : base("ModelInterface", "Sample model definition")
	{
		this.Field("stringProp", e => e.StringProperty);
	}
}

public class ModelTypeWithIntProp : GraphQLObjectType<ModelWithIntProp>
{
	public ModelType() : base("ModelWithInt", "Sample model definition")
	{
        this.Field("stringProp", e => e.StringProperty);
        this.Field("intProp", e => e.AdditionalIntProperty);
	}
}

public class ModelTypeWithBoolProp : GraphQLObjectType<ModelWithBoolProp>
{
	public ModelType() : base("ModelWithBool", "Sample model definition")
	{
        this.Field("stringProp", e => e.StringProperty);
        this.Field("boolProp", e => e.AdditionalBoolProperty);
	}
}

public class Query : GraphQLObjectType
{
	public Query() : base("Query", "Sample root query defintion")
	{
		this.Field("models", () => this.GetModels());
	}

    private IModel GetModel()
    {
        return IModel[] {
            new ModelWithIntProp()
            {
                StringProperty = "Test",
                AdditionalIntProperty = 123
            };
            new ModelWithBoolProp()
            {
                StringProperty = "Test",
                AdditionalBoolProperty = true
            };
        }
    }
}
```
### Query

```graphql
{
  models {
    stringProp
    ... on ModelWithInt {
      intProp
    }
    ... on ModelWithBool {
      boolProp
    }
  }
}
```

### Result

```json
{
  "model" : [
    { "stringProp" : "Test", "intProp" : 123 },
    { "stringProp" : "Test", "boolProp" : true }
  ]
}
```

## Accessors
On interfaces you can define only accessors. For more information about
accessors have a look into the
[typed object definition section](../typed-object-definition/overview.md#accessors).