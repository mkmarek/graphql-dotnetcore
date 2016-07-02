# Untyped object definition

In general you have two option how to define GraphQL objects.
This section describes objects defined without any binding to model classes.
These objects could be used for example for root queries.

## Example
### Definition

```csharp
public class Query : GraphQLObjectType
{
	public Query(GraphQLSchema schema) 
		: base("Query", "Sample root query defintion", schema)
	{
		this.Field("sum", (int[] numbers) => numbers.Sum());
	}
}
```

### Query

```graphql
{
  sum(numbers: [1,2,3])
}
```

### Result

```json
{
  "sum" : 6
}
```

## Resolvers

Resolvers are methods returning either scalar, interface or object result.
By defining parameters in resolver you are defining arguments that can
be used as an input values inside GraphQL query.
Types of those values are automatically translated to GraphQL data
types.

This can be seen while introspecting the sum method defined above.

### Query

```graphql
{
  __type(name: "Query") {
    kind
    name
    description
    fields {
      name
      type {
        ...typeFragment
      }
      args {
        name,
        type {
          ...typeFragment
        }
      }
    }
  }
}

fragment typeFragment on __Type {
  kind
  ofType {
    kind
    name
    ofType {
      kind
      name
    }
  }
}
```

### Result

```json
{
  "data": {
    "__type": {
      "kind": "OBJECT",
      "name": "Query",
      "description": "Sample root query defintion",
      "fields": [
        {
          "name": "sum",
          "type": {
            "kind": "NON_NULL",
            "ofType": {
              "kind": "SCALAR",
              "name": "Int",
              "ofType": null
            }
          },
          "args": [
            {
              "name": "numbers",
              "type": {
                "kind": "LIST",
                "ofType": {
                  "kind": "NON_NULL",
                  "name": null,
                  "ofType": {
                    "kind": "SCALAR",
                    "name": "Int"
                  }
                }
              }
            }
          ]
        }
      ]
    }
  }
}
```