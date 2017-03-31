namespace GraphQLCore.Type.Complex
{
    using Execution;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Translation;
    using Utils;

    public class GraphQLSubscriptionTypeFieldInfo : GraphQLObjectTypeFieldInfo
    {
        public Type SubscriptionReturnType { get; set; }
        public LambdaExpression Filter { get; set; }

        public new static GraphQLSubscriptionTypeFieldInfo CreateResolverFieldInfo(string fieldName, LambdaExpression resolver)
        {
            return new GraphQLSubscriptionTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = true,
                Lambda = resolver,
                Arguments = GetArguments(resolver),
                SystemType = typeof(long),
                SubscriptionReturnType = ReflectionUtilities.GetReturnValueFromLambdaExpression(resolver)
            };
        }
    }
}