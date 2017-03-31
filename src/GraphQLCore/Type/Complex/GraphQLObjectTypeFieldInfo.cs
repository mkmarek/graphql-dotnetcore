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

    public class GraphQLObjectTypeFieldInfo : GraphQLInputObjectTypeFieldInfo
    {
        public bool IsResolver { get; set; }
        public string Channel {get; set;}

        public static new GraphQLObjectTypeFieldInfo CreateAccessorFieldInfo(string fieldName, LambdaExpression accessor)
        {
            return new GraphQLObjectTypeFieldInfo()
            {
                Name = fieldName,
                IsResolver = false,
                Arguments = new Dictionary<string, GraphQLObjectTypeArgumentInfo>(),
                Lambda = accessor,
                SystemType = ReflectionUtilities.GetReturnValueFromLambdaExpression(accessor)
            };
        }

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

        protected override GraphQLBaseType GetSchemaType(Type type, ISchemaRepository schemaRepository)
        {
            return schemaRepository.GetSchemaTypeFor(type);
        }

        protected static Dictionary<string, GraphQLObjectTypeArgumentInfo> GetArguments(LambdaExpression resolver)
        {
            var contextType = typeof(IContext<>);

            return resolver.Parameters
                .Where(e => !(e.Type.GetTypeInfo().IsGenericType && e.Type.GetGenericTypeDefinition() == contextType))
                .Select(e => new GraphQLObjectTypeArgumentInfo()
            {
                Name = e.Name,
                SystemType = e.Type
            }).ToDictionary(e => e.Name);
        }
    }
}