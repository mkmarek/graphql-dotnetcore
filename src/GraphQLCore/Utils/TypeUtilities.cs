namespace GraphQLCore.Utils
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Type;
    using Type.Introspection;
    using Type.Scalars;

    public static class TypeUtilities
    {
        public static object ChangeValueType(object input, ParameterExpression parameter)
        {
            if (input is IEnumerable<object> && ReflectionUtilities.IsCollection(parameter.Type))
                return ReflectionUtilities.ChangeToCollection(input, parameter);

            return TryConvertToParameterType(input, parameter);
        }

        public static object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => ChangeValueType(GetArgumentValue(arguments, e.Name), e))
                .ToArray();
        }

        public static __InputValue[] FetchInputArguments(LambdaExpression expression, GraphQLSchema schema)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => new __InputValue(e, schema))
                .ToArray();
        }

        public static object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName)
        {
            var value = arguments.SingleOrDefault(e => e.Name.Value == argumentName).Value;

            return GetValue(value);
        }

        public static IEnumerable GetListValue(GraphQLValue value)
        {
            IList output = new List<object>();
            var list = ((GraphQLValue<IEnumerable<GraphQLValue>>)value).Value;

            foreach (var item in list)
                output.Add(GetValue(item));

            return output;
        }

        public static string GetTypeKind(__Type e)
        {
            return (string)e.ResolveField("kind");
        }

        public static string GetTypeName(__Type e)
        {
            return (string)e.ResolveField("name");
        }

        public static string[] GetTypeNames(List<__Type> typeList)
        {
            return typeList.Select(e => GetTypeName(e)).ToArray();
        }

        public static IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value, GraphQLSchema schema)
        {
            var types = value.GetFieldTypes();

            return types
                .Select(e => ResolveObjectFieldType(e, schema))
                .Where(e => e != null);
        }

        public static object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression)
        {
            try
            {
                var argumentValues = FetchArgumentValues(expression, arguments);

                return expression.Compile().DynamicInvoke(argumentValues);
            }catch (Exception ex)
            {
                throw ex;
            }
        }

        public static __Type ResolveObjectFieldType(System.Type type, GraphQLSchema schema)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null), schema);

            if (typeof(bool) == type)
                return new __Type(new GraphQLBoolean(null), schema);

            if (typeof(float) == type || typeof(double) == type)
                return new __Type(new GraphQLFloat(null), schema);

            if (typeof(string) == type)
                return new __Type(new GraphQLString(null), schema);

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

        public static GraphQLScalarType GetElementFromSchema(Type type, GraphQLSchema schema)
        {
            return schema.SchemaTypes.FirstOrDefault(e => e.GetType() == type);
        }

        public static GraphQLScalarType GetEnumElementFromSchema(Type type, GraphQLSchema schema)
        {
            return schema.SchemaTypes
                .Where(e => e is GraphQLEnumType)
                .Select(e => (GraphQLEnumType)e)
                .FirstOrDefault(e => e.IsOfType(type));
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

        public static object TryConvertToParameterType(object input, ParameterExpression parameter)
        {
            try
            {
                if (parameter.Type.GetTypeInfo().IsEnum)
                    return TryConvertToEnumParameterType(input, parameter.Type);

                return Convert.ChangeType(input, parameter.Type);
            }
            catch (Exception ex)
            {
                throw new GraphQLException($"Can't convert input of type {input.GetType().Name} to {parameter.Type.Name}.", ex);
            }
        }

        private static object TryConvertToEnumParameterType(object input, Type type)
        {
            return Enum.Parse(type, input as string);
        }

        private static object GetValue(GraphQLValue value)
        {
            switch (value.Kind)
            {
                case ASTNodeKind.BooleanValue: return ((GraphQLValue<bool>)value).Value;
                case ASTNodeKind.IntValue: return ((GraphQLValue<int>)value).Value;
                case ASTNodeKind.FloatValue: return ((GraphQLValue<float>)value).Value;
                case ASTNodeKind.StringValue: return ((GraphQLValue<string>)value).Value;
                case ASTNodeKind.EnumValue: return ((GraphQLValue<string>)value).Value;
                case ASTNodeKind.ListValue: return GetListValue(value);
            }

            throw new NotImplementedException();
        }
    }
}