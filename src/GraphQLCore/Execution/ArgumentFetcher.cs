namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Type;
    using Type.Translation;
    using Utils;

    public class ArgumentFetcher : IArgumentFetcher
    {
        private readonly ISchemaRepository schemaRepository;

        public ArgumentFetcher(ISchemaRepository schemaRepository)
        {
            this.schemaRepository = schemaRepository;
        }

        public object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments, object parent)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => this.GetValueForArgument(arguments, e, parent))
                .ToArray();
        }

        private static bool IsContextType(ParameterExpression e)
        {
            var contextType = typeof(IContext<>);

            return e.Type.GetTypeInfo().IsGenericType && e.Type.GetGenericTypeDefinition() == contextType;
        }

        private object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName, GraphQLInputType type)
        {
            var argument = arguments.SingleOrDefault(e => e.Name.Value == argumentName);

            if (argument == null)
                return null;

            return type.GetFromAst(argument.Value, this.schemaRepository).Value;
        }

        private object GetValueForArgument(IList<GraphQLArgument> arguments, ParameterExpression e, object parent)
        {
            if (IsContextType(e))
                return this.CreateContextObject(e.Type, parent);

            return ReflectionUtilities.ChangeValueType(
                this.GetArgumentValue(
                    arguments,
                    e.Name,
                    this.schemaRepository.GetSchemaInputTypeFor(e.Type)),
                e.Type);
        }

        private object CreateContextObject(Type type, object parent)
        {
            var genericArgument = type.GetTypeInfo()
                .GetGenericArguments()
                .Single();

            var fieldContextType = typeof(FieldContext<>)
                .MakeGenericType(genericArgument);

            return Activator.CreateInstance(fieldContextType, parent);
        }
    }
}
