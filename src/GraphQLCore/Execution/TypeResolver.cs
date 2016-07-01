namespace GraphQLCore.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Type;
    using Type.Introspection;
    using Type.Scalars;
    using Utils;

    public class TypeResolver
    {
        public static GraphQLScalarType GetElementFromSchema(Type type, GraphQLSchema schema)
        {
            return schema.SchemaTypes.FirstOrDefault(e => e.GetType() == type);
        }

        public static GraphQLScalarType GetElementFromSchemaByModelType(Type type, GraphQLSchema schema)
        {
            var modelType = schema.SchemaTypes.FirstOrDefault(e =>
                ReflectionUtilities.GetGenericArgumentsFromAllParents(e.GetType()).Contains(type));

            if (modelType != null)
                return modelType;

            var typeParents = ReflectionUtilities.GetAllParentsAndCurrentTypeFrom(type);
            typeParents.AddRange(ReflectionUtilities.GetAllImplementingInterfaces(type));

            return schema.SchemaTypes.FirstOrDefault(e => typeParents.Any(t =>
            ReflectionUtilities.GetGenericArgumentsFromAllParents(e.GetType()).Contains(t)));
        }

        public static GraphQLScalarType GetEnumElementFromSchema(Type type, GraphQLSchema schema)
        {
            return schema.SchemaTypes
                .Where(e => e is GraphQLEnumType)
                .Select(e => (GraphQLEnumType)e)
                .FirstOrDefault(e => e.IsOfType(type));
        }

        public static __Type ResolveGraphType(Type type, GraphQLSchema schema)
        {
            var scalarType = ResolveScalarTypes(type, schema);
            if (scalarType != null)
                return scalarType;

            if (ReflectionUtilities.IsCollection(type))
                return new __Type(new GraphQLList(type, schema), schema);

            if (type.GetTypeInfo().IsEnum)
                return new __Type(GetEnumElementFromSchema(type, schema), schema);

            var schemaType = GetElementFromSchema(type, schema);
            if (schemaType != null)
                return new __Type(schemaType, schema);

            schemaType = GetElementFromSchemaByModelType(type, schema);
            if (schemaType != null)
                return new __Type(schemaType, schema);

            return null;
        }

        public static __Type ResolveObjectArgumentType(System.Type type, GraphQLSchema schema)
        {
            if (ReflectionUtilities.IsCollection(type))
                return ResolveGraphType(type, schema);

            if (ReflectionUtilities.IsClass(type))
                return ResolveGraphType(type, schema);

            if (type == typeof(string))
                return ResolveGraphType(type, schema);

            if (ReflectionUtilities.IsStruct(type))
                return new __Type(ResolveGraphType(type, schema), schema);

            var underlyingNullableType = Nullable.GetUnderlyingType(type);
            if (underlyingNullableType == null)
                return new __Type(ResolveGraphType(type, schema), schema);

            return ResolveGraphType(underlyingNullableType, schema);
        }

        public static IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value, GraphQLSchema schema)
        {
            var types = value.GetFieldTypes();

            return types
                .Select(e =>
                ResolveScalarTypes(e, schema) ??
                ResolveObjectArgumentType(e, schema))
                .Where(e => e != null);
        }

        private static __Type ResolveScalarTypes(Type type, GraphQLSchema schema)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null), schema);

            if (typeof(bool) == type)
                return new __Type(new GraphQLBoolean(null), schema);

            if (typeof(float) == type || typeof(double) == type)
                return new __Type(new GraphQLFloat(null), schema);

            if (typeof(string) == type)
                return new __Type(new GraphQLString(null), schema);

            return null;
        }
    }
}