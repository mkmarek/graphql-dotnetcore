namespace GraphQLCore.Type
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Utils;

    public class GraphQLObjectTypeFieldInfo
    {
        public IDictionary<string, GraphQLObjectTypeArgumentInfo> Arguments { get; set; }
        public bool IsResolver { get; set; }
        public LambdaExpression Lambda { get; set; }
        public string Name { get; set; }
        public System.Type SystemType { get; set; }

        public static GraphQLObjectTypeFieldInfo CreateResolverFieldInfo(string fieldName, LambdaExpression resolver)
        {
            return new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = true,
                Lambda = resolver,
                Arguments = GetArguments(resolver),
                SystemType = ReflectionUtilities.GetReturnValueFromLambdaExpression(resolver)
            };
        }

        public static GraphQLObjectTypeFieldInfo CreateAccessorFieldInfo(string fieldName, LambdaExpression accessor)
        {
            return new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = false,
                Lambda = accessor,
                Arguments = new Dictionary<string, GraphQLObjectTypeArgumentInfo>(),
                SystemType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor)
            };
        }

        private static Dictionary<string, GraphQLObjectTypeArgumentInfo> GetArguments(LambdaExpression resolver)
        {
            return resolver.Parameters.Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                Type = e.Type
            }).ToDictionary(e => e.Name);
        }
    }
}