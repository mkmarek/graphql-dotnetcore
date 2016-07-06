//namespace GraphQLCore.Execution
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Reflection;
//    using Type;
//    using Type.Introspection;
//    using Type.Scalars;
//    using Utils;

//    public class TypeResolver
//    {
//        public static GraphQLScalarType GetElementFromSchema(Type type, GraphQLSchema schema)
//        {
//            return schema.SchemaTypes.FirstOrDefault(e => e.GetType() == type);
//        }

//        public static GraphQLScalarType GetElementFromSchemaByModelType(Type type, GraphQLSchema schema)
//        {
//            var modelType = schema.SchemaTypes.FirstOrDefault(e =>
//                ReflectionUtilities.GetGenericArgumentsFromAllParents(e.GetType()).Contains(type));

//            if (modelType != null)
//                return modelType;

//            var typeParents = ReflectionUtilities.GetAllParentsAndCurrentTypeFrom(type);
//            typeParents.AddRange(ReflectionUtilities.GetAllImplementingInterfaces(type));

//            return schema.SchemaTypes.FirstOrDefault(e => typeParents.Any(t =>
//            ReflectionUtilities.GetGenericArgumentsFromAllParents(e.GetType()).Contains(t)));
//        }

//        public static GraphQLScalarType GetInterfaceFromSchemaByInterfaceType(Type type, GraphQLSchema schema)
//        {
//            return schema.SchemaTypes.FirstOrDefault(e =>
//                ReflectionUtilities.GetGenericArguments(e.GetType()).Contains(type));
//        }

//        public static GraphQLScalarType GetEnumElementFromSchema(Type type, GraphQLSchema schema)
//        {
//            return schema.SchemaTypes
//                .Where(e => e is GraphQLEnumType)
//                .Select(e => (GraphQLEnumType)e)
//                .FirstOrDefault(e => e.IsOfType(type));
//        }

//        public static __Type ResolveInterfaceGraphType(Type type, GraphQLSchema schema)
//        {
//            var schemaType = GetInterfaceFromSchemaByInterfaceType(type, schema);
//            if (schemaType != null)
//                return new __Type(schemaType, schema);

//            return null;
//        }

//        public static GraphQLScalarType ResolveGraphType(Type type, GraphQLSchema schema)
//        {
//            var scalarType = ResolveScalarType(type);
//            if (scalarType != null)
//                return scalarType;

//            //if (ReflectionUtilities.IsCollection(type))
//            //    return new GraphQLList(type);

//            if (type.GetTypeInfo().IsEnum)
//                return GetEnumElementFromSchema(type, schema);

//            var schemaType = GetElementFromSchema(type, schema);
//            if (schemaType != null)
//                return schemaType;

//            schemaType = GetElementFromSchemaByModelType(type, schema);
//            if (schemaType != null)
//                return schemaType;

//            return null;
//        }

//        public static __Type IntrospectObjectArgumentType(System.Type type, GraphQLSchema schema)
//        {
//            if (ReflectionUtilities.IsCollection(type))
//                return new __Type(ResolveGraphType(type, schema), schema);

//            if (ReflectionUtilities.IsClass(type))
//                return new __Type(ResolveGraphType(type, schema), schema);

//            if (type == typeof(string))
//                return new __Type(ResolveGraphType(type, schema), schema);

//            if (ReflectionUtilities.IsStruct(type))
//                return new __Type(new __Type(ResolveGraphType(type, schema), schema), schema);

//            var underlyingNullableType = Nullable.GetUnderlyingType(type);
//            if (underlyingNullableType == null)
//                return new __Type(new __Type(ResolveGraphType(type, schema), schema), schema);

//            return new __Type(ResolveGraphType(underlyingNullableType, schema), schema);
//        }

//        public static IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value, GraphQLSchema schema)
//        {
//            var types = value.GetFieldsInfo();

//            return types
//                .Select(e =>
//                IntrospectScalarType(e.ReturnValueType, schema) ??
//                IntrospectObjectArgumentType(e.ReturnValueType, schema))
//                .Where(e => e != null);
//        }

//        private static __Type IntrospectScalarType(Type type, GraphQLSchema schema)
//        {
//            var scalar = ResolveScalarType(type);

//            if (scalar == null)
//                return null;

//            return new __Type(scalar, schema);
//        }

//        private static GraphQLScalarType ResolveScalarType(Type type)
//        {
//            if (typeof(int) == type)
//                return new GraphQLInt();

//            if (typeof(bool) == type)
//                return new GraphQLBoolean();

//            if (typeof(float) == type || typeof(double) == type)
//                return new GraphQLFloat();

//            if (typeof(string) == type)
//                return new GraphQLString();

//            return null;
//        }
//    }
//}