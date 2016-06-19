using System;
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

        public static string[] GetParameterNames(LambdaExpression resolver)
        {
            return resolver.Parameters.Select(e => e.Name).ToArray();
        }
    }
}
