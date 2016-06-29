using System.Linq;

namespace GraphQLCore.Type.Introspection
{
    public class __Type : GraphQLObjectType
    {
        public __Type(GraphQLScalarType type, GraphQLSchema schema) : this(type.Name, type.Description, schema)
        {
            this.Field("kind", () => this.ResolveKind(type).ToString());
            this.Field("fields", () => this.IfObjectGetFields(type));
            this.Field("enumValues", () => this.IfEnumGetValues(type));
            this.Field("ofType", () => this.ResolveOfType(type));
            this.Field("interfaces", () => this.IfObjectResolveInterfaces(type));
        }

        public __Type(string fieldName, string fieldDescription, GraphQLSchema schema) : base("__Type", "The fundamental unit of any GraphQL Schema is the type.There are " +
            "many kinds of types in GraphQL as represented by the `__TypeKind` enum." +
            "\n\nDepending on the kind of a type, certain fields describe " +
            "information about that type. Scalar types provide no information " +
            "beyond a name and description, while Enum types provide their values. " +
            "Object and Interface types provide the fields they describe. Abstract " +
            "types, Union and Interface, provide the Object types possible " +
            "at runtime. List and NonNull types compose other types.", null)
        {
            this.schema = schema;
            this.Field("name", () => fieldName);
            this.Field("description", () => fieldDescription);
            //this.Field("interfaces", () => new GraphQLInterface[] { });
        }

        private __Field[] GetFieldsFromObject(GraphQLObjectType type)
        {
            return type.IntrospectFields();
        }

        private GraphQLEnumValue[] GetValuesFromEnum(GraphQLEnumType type)
        {
            return type.GetEnumValues().ToArray();
        }

        private GraphQLEnumValue[] IfEnumGetValues(GraphQLScalarType type)
        {
            if (type is GraphQLEnumType)
            {
                return this.GetValuesFromEnum((GraphQLEnumType)type);
            }

            return null;
        }

        private __Field[] IfObjectGetFields(GraphQLScalarType type)
        {
            if (type is GraphQLObjectType)
            {
                return this.GetFieldsFromObject((GraphQLObjectType)type);
            }

            return null;
        }

        private object IfObjectResolveInterfaces(GraphQLScalarType type)
        {
            if (type is GraphQLObjectType)
            {
                return new string[] { };
            }

            return null;
        }

        private TypeKind ResolveKind(GraphQLScalarType type)
        {
            if (type is GraphQLList)
                return TypeKind.LIST;

            if (type is GraphQLEnumType)
                return TypeKind.ENUM;

            if (type is GraphQLObjectType)
                return TypeKind.OBJECT;

            return TypeKind.SCALAR;
        }

        private object ResolveOfType(GraphQLScalarType type)
        {
            if (type is GraphQLList)
            {
                return ((GraphQLList)type).GetMemberType();
            }

            return null;
        }
    }
}