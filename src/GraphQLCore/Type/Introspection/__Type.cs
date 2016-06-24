using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLCore.Type.Scalars;

namespace GraphQLCore.Type.Introspection
{
    public class __Type : GraphQLObjectType
    {
        private TypeKind kind;
        private __Field[] fields;

        public __Type(GraphQLScalarType type, GraphQLSchema schema) : this(type.Name, type.Description, schema)
        {
            this.kind = this.ResolveKind(type);
            this.fields = this.IfObjectGetFields(type);
        }

        private __Field[] IfObjectGetFields(GraphQLScalarType type)
        {
            if (type is GraphQLObjectType)
            {
                return this.GetFieldsFromObject((GraphQLObjectType)type);
            }

            return new __Field[] { };
        }

        private __Field[] GetFieldsFromObject(GraphQLObjectType type)
        {
            return type.IntrospectFields();
        }

        public __Type(string fieldName, string fieldDescription, GraphQLSchema schema) : base("__Type", "The fundamental unit of any GraphQL Schema is the type.There are " +
            "many kinds of types in GraphQL as represented by the `__TypeKind` enum." +
            "\n\nDepending on the kind of a type, certain fields describe " +
            "information about that type. Scalar types provide no information " +
            "beyond a name and description, while Enum types provide their values. " +
            "Object and Interface types provide the fields they describe. Abstract " +
            "types, Union and Interface, provide the Object types possible " +
            "at runtime. List and NonNull types compose other types.", schema)
        {

            this.Field("name", () => fieldName);
            this.Field("kind", () => this.kind.ToString());
            this.Field("description", () => fieldDescription);
            this.Field("fields", () => fields);
            this.Field("interfaces", () => new GraphQLInterface[] { });
        }

        private TypeKind ResolveKind(GraphQLScalarType type) 
        {
            if (type is GraphQLObjectType)
                return TypeKind.OBJECT;

            return TypeKind.SCALAR;
        }
    }
}
