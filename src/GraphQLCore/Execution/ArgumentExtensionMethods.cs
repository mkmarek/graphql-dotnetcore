namespace GraphQLCore.Execution
{
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Translation;

    public static class ArgumentExtensionMethods
    {
        public static List<GraphQLArgument> GetArgumentsWithDefaultValues(
            this GraphQLDirective directive,
            GraphQLDirectiveType directiveInfo,
            ISchemaRepository schemaRepository)
        {
            if (directiveInfo == null || directiveInfo.Arguments?.Count == 0)
                return null;

            return MergeArgumentsWithDefault(directive.Arguments, directiveInfo.Arguments, schemaRepository);
        }

        public static List<GraphQLArgument> GetArgumentsWithDefaultValues(
            this GraphQLFieldSelection selection,
            GraphQLFieldInfo fieldInfo,
            ISchemaRepository schemaRepository)
        {
            if (fieldInfo == null || fieldInfo.Arguments?.Count == 0)
                return null;

            return MergeArgumentsWithDefault(selection.Arguments, fieldInfo.Arguments, schemaRepository);
        }

        private static List<GraphQLArgument> MergeArgumentsWithDefault(
            IEnumerable<GraphQLArgument> arguments,
            IDictionary<string, GraphQLObjectTypeArgumentInfo> argumentInfos,
            ISchemaRepository schemaRepository)
        {
            var combinedArguments = arguments.ToDictionary(e => e.Name.Value, e => e);

            foreach (var argument in argumentInfos)
            {
                var argumentName = argument.Key;
                var argumentValue = argument.Value;

                if (!combinedArguments.ContainsKey(argumentName) && argumentValue.DefaultValue.IsSet)
                {
                    var defaultArgument = new GraphQLArgument()
                    {
                        Name = new GraphQLName() { Value = argumentName },
                        Value = argumentValue.DefaultValue.GetAstValue(
                            (GraphQLInputType)argumentValue.GetGraphQLType(schemaRepository), schemaRepository)
                    };
                    combinedArguments.Add(argumentName, defaultArgument);
                }
            }

            return combinedArguments.Values.ToList();
        }
    }
}
