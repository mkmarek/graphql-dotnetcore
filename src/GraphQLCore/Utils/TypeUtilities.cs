namespace GraphQLCore.Utils
{
    using Exceptions;
    using Language.AST;
    using System;
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

        public static GraphQLInputArgument[] FetchInputArguments(LambdaExpression expression)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => new GraphQLInputArgument(e))
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

        public static IEnumerable<__Type> IntrospectObjectFieldTypes(GraphQLObjectType value)
        {
            var types = value.GetFieldTypes();

            return types
                .Select(e => ResolveObjectFieldType(e))
                .Where(e => e != null);
        }

        public static object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression)
        {
            var argumentValues = FetchArgumentValues(expression, arguments);

            return expression.Compile().DynamicInvoke(argumentValues);
        }

        public static __Type ResolveObjectFieldType(System.Type type)
        {
            if (typeof(int) == type)
                return new __Type(new GraphQLInt(null));

            if (typeof(bool) == type)
                return new __Type(new GraphQLBoolean(null));

            if (typeof(float) == type || typeof(double) == type)
                return new __Type(new GraphQLFloat(null));

            if (typeof(string) == type)
                return new __Type(new GraphQLString(null));

            if (ReflectionUtilities.IsCollection(type))
                return new __Type(new GraphQLList(type));

            return null;
        }

        public static object TryConvertToParameterType(object input, ParameterExpression parameter)
        {
            try
            {
                return Convert.ChangeType(input, parameter.Type);
            }
            catch (Exception ex)
            {
                throw new GraphQLException($"Can't convert input of type {input.GetType().Name} to {parameter.Type.Name}.", ex);
            }
        }

        private static object GetValue(GraphQLValue value)
        {
            switch (value.Kind)
            {
                case ASTNodeKind.BooleanValue: return ((GraphQLValue<bool>)value).Value;
                case ASTNodeKind.IntValue: return ((GraphQLValue<int>)value).Value;
                case ASTNodeKind.FloatValue: return ((GraphQLValue<float>)value).Value;
                case ASTNodeKind.StringValue: return ((GraphQLValue<string>)value).Value;
                case ASTNodeKind.ListValue: return GetListValue(value);
            }

            throw new NotImplementedException();
        }
    }
}