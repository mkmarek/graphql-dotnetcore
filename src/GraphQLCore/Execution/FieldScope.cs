namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Type;
    using Type.Introspection;
    using Utils;

    public class FieldScope
    {
        private List<GraphQLArgument> arguments;
        private ExecutionContext context;
        private object parent;
        private GraphQLObjectType type;

        public FieldScope(ExecutionContext context, GraphQLObjectType type, object parent)
        {
            this.type = type;
            this.context = context;
            this.parent = parent;
            this.arguments = new List<GraphQLArgument>();
        }

        public dynamic GetObject(Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                foreach (var selection in field.Value)
                    AddToResultDictionaryIfNotAlreadyPresent(dictionary, field.Key, selection);

            return result;
        }

        private void AddToResultDictionaryIfNotAlreadyPresent(
            IDictionary<string, object> dictionary, string fieldName, GraphQLFieldSelection selection)
        {
            if (!dictionary.ContainsKey(fieldName))
                dictionary.Add(fieldName, GetDefinitionAndExecuteField(type, selection));
        }

        private object CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            var result = new List<object>();
            foreach (var element in input)
                result.Add(this.CompleteValue(element, selection, arguments));

            return result;
        }

        private object CompleteObjectType(GraphQLObjectType input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments, object parentObject)
        {
            var scope = this.context.CreateScope(input, parentObject);
            scope.arguments = arguments.ToList();

            return this.context.GetResultFromScope(input, selection.SelectionSet, scope);
        }

        private object CompleteValue(object input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            if (input == null)
                return null;

            if (input is GraphQLObjectType)
                return this.CompleteObjectType((GraphQLObjectType)input, selection, arguments, this.parent);

            if (ReflectionUtilities.IsCollection(input.GetType()))
                return this.CompleteCollectionType((IEnumerable)input, selection, arguments);

            var schemaValue = this.context.GraphQLSchema.TypeTranslator.GetType(input.GetType());
            if (schemaValue is GraphQLObjectType)
            {
                return this.CompleteObjectType((GraphQLObjectType)schemaValue, selection, arguments, input);
            }

            return input;
        }

        private object ExecuteField(Func<IList<GraphQLArgument>, object> fieldResolver, GraphQLFieldSelection selection)
        {
            var args = arguments.ToList();
            args.RemoveAll(e => selection.Arguments.Any(arg => arg.Name.Value.Equals(e.Name.Value)));
            args.AddRange(selection.Arguments);

            return this.CompleteValue(fieldResolver(args), selection, args);
        }

        private object GetDefinitionAndExecuteField(GraphQLObjectType type, GraphQLFieldSelection selection)
        {
            return this.ExecuteField(GetFieldDefinition(type, selection), selection);
        }

        private Func<IList<GraphQLArgument>, object> GetFieldDefinition(GraphQLObjectType type, GraphQLFieldSelection selection)
        {
            if (selection.Name.Value == "__schema")
                return (args) => this.context.GraphQLSchema.IntrospectedSchema;

            if (selection.Name.Value == "__type")
                return (args) => TypeUtilities.InvokeWithArguments(args, this.GetTypeIntrospectionExpression());

            return (args) => type.ResolveField(selection, args, parent);
        }

        private Expression<Func<string, IntrospectedType>> GetTypeIntrospectionExpression()
        {
            return (string name) => this.context.GraphQLSchema.GetGraphQLType(name);
        }
    }
}