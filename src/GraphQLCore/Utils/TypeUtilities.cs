namespace GraphQLCore.Utils
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class TypeUtilities
    {
        public static object ChangeValueType(object input, ParameterExpression parameter)
        {
            if (input is IEnumerable<object> && ReflectionUtilities.IsCollection(parameter.Type))
                return ReflectionUtilities.ChangeToCollection(input, parameter);

            return TryConvertToParameterType(input, parameter);
        }

        public static object ConvertTo(object input, Type target)
        {
            if (target.GetTypeInfo().IsEnum)
                return TryConvertToEnumParameterType(input, target);

            return Convert.ChangeType(input, target);
        }

        public static object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => ChangeValueType(GetArgumentValue(arguments, e.Name), e))
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

        public static object GetValue(GraphQLValue value)
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

        public static object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression)
        {
            var argumentValues = FetchArgumentValues(expression, arguments);

            return expression.Compile().DynamicInvoke(argumentValues);
        }

        public static object TryConvertToParameterType(object input, ParameterExpression parameter)
        {
            try
            {
                return ConvertTo(input, parameter.Type);
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
    }
}