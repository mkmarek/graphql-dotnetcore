using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphQL.Utils
{
    public class ReflectionUtilities
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            System.Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            return propInfo;
        }

        public static System.Type GetReturnValueFromLambdaExpression(LambdaExpression expression)
        {
            return expression.Type.GenericTypeArguments.LastOrDefault();
        }

        public static ParameterExpression[] GetParameters(LambdaExpression resolver)
        {
            return resolver.Parameters.ToArray();
        }

        public static object Cast(System.Type type, object input)
        {
            var cast = typeof(Enumerable).GetRuntimeMethod("Cast", new System.Type[] { typeof(IEnumerable) })
                .MakeGenericMethod(type);

            return cast.Invoke(null, new object[] { input });
        }

        internal static bool IsCollection(System.Type type)
        {
            return (type.IsArray || typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())) && type != typeof(string);
        }

        public static object ToArray(System.Type type, object input)
        {
            var toArray = typeof(Enumerable).GetRuntimeMethods()
                .SingleOrDefault(e => e.Name == "ToArray")
                .MakeGenericMethod(type);

            return toArray.Invoke(null, new object[] { input });
        }

        public static object ToList(System.Type type, object input)
        {
            var toList = typeof(Enumerable).GetRuntimeMethods()
                .SingleOrDefault(e => e.Name == "ToList")
                .MakeGenericMethod(type);

            return toList.Invoke(null, new object[] { input });
        }

        public static object ChangeToCollection(object input, ParameterExpression parameter)
        {
            if (parameter.Type.IsArray)
                return ChangeToArrayCollection(input, parameter);

            return ChangeToListCollection(input, parameter);
        }

        public static object ChangeToArrayCollection(object input, ParameterExpression parameter)
        {
            var elementType = parameter.Type.GetElementType();

            return ToArray(elementType, Cast(elementType, input));
        }

        public static object ChangeToListCollection(object input, ParameterExpression parameter)
        {
            var elementType = parameter.Type.GenericTypeArguments.Single();

            return ToList(elementType, Cast(elementType, input));
        }
    }
}
