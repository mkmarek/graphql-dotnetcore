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
    using Type.Translation;
    using Utils;

    public class FieldScope
    {
        private List<GraphQLArgument> arguments;
        private object parent;
        private GraphQLObjectType type;
        private IValueResolver valueResolver;
        private ITypeTranslator typeTranslator;
        private IFieldCollector fieldCollector;

        public FieldScope(
            ITypeTranslator typeTranslator,
            IValueResolver valueResolver,
            IFieldCollector fieldCollector,
            GraphQLObjectType type,
            object parent)
        {
            this.type = type;
            this.parent = parent;
            this.arguments = new List<GraphQLArgument>();

            this.fieldCollector = fieldCollector;
            this.typeTranslator = typeTranslator;
            this.valueResolver = valueResolver;
        }

        public dynamic GetObject(Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value);

            return result;
        }

        private void AddFieldsFromSelectionToResultDictionary(IDictionary<string, object> dictionary, string fieldName, IList<GraphQLFieldSelection> fieldSelections)
        {
            foreach (var selection in fieldSelections)
                this.AddToResultDictionaryIfNotAlreadyPresent(dictionary, fieldName, selection);
        }

        private void AddToResultDictionaryIfNotAlreadyPresent(
                    IDictionary<string, object> dictionary, string fieldName, GraphQLFieldSelection selection)
        {
            if (!dictionary.ContainsKey(fieldName))
                dictionary.Add(fieldName, this.GetDefinitionAndExecuteField(this.type, selection));
        }

        private object CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            var result = new List<object>();
            foreach (var element in input)
                result.Add(this.CompleteValue(element, selection, arguments));

            return result;
        }

        private object CompleteObjectType(
            GraphQLObjectType input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments, object parentObject)
        {
            var scope = new FieldScope(
                this.typeTranslator,
                this.valueResolver,
                this.fieldCollector,
                input,
                parentObject);

            scope.arguments = arguments.ToList();

            return scope.GetObject(this.fieldCollector.CollectFields(input, selection.SelectionSet));
        }

        private object CompleteValue(object input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            if (input == null)
                return null;

            if (input is GraphQLObjectType)
                return this.CompleteObjectType((GraphQLObjectType)input, selection, arguments, this.parent);

            if (ReflectionUtilities.IsCollection(input.GetType()))
                return this.CompleteCollectionType((IEnumerable)input, selection, arguments);

            var schemaValue = this.typeTranslator.GetType(input.GetType());
            if (schemaValue is GraphQLObjectType)
            {
                return this.CompleteObjectType((GraphQLObjectType)schemaValue, selection, arguments, input);
            }

            if (ReflectionUtilities.IsEnum(input.GetType()))
                return input.ToString();

            return input;
        }

        private object GetDefinitionAndExecuteField(GraphQLObjectType type, GraphQLFieldSelection selection)
        {
            var arguments = this.GetArgumentsFromSelection(selection);
            var fieldInfo = this.GetFieldInfo(type, selection);
            var resolvedValue = this.ResolveField(fieldInfo, arguments, this.parent);

            return this.CompleteValue(resolvedValue, selection, arguments);
        }

        private List<GraphQLArgument> GetArgumentsFromSelection(GraphQLFieldSelection selection)
        {
            var arguments = this.arguments.ToList();

            arguments.RemoveAll(e => selection.Arguments.Any(arg => arg.Name.Value.Equals(e.Name.Value)));
            arguments.AddRange(selection.Arguments);

            return arguments;
        }

        private GraphQLObjectTypeFieldInfo GetFieldInfo(GraphQLObjectType type, GraphQLFieldSelection selection)
        {
            var name = this.GetFieldName(selection);
            return type.GetFieldInfo(name);
        }

        private string GetFieldName(GraphQLFieldSelection selection)
        {
            return selection.Name?.Value ?? selection.Alias?.Value;
        }

        private object ProcessField(object input)
        {
            if (input == null)
                return null;

            if (ReflectionUtilities.IsEnum(input.GetType()))
                return input.ToString();

            return input;
        }

        private object ResolveField(GraphQLObjectTypeFieldInfo fieldInfo, IList<GraphQLArgument> arguments, object parent)
        {
            if (fieldInfo == null)
                return null;

            if (fieldInfo.IsResolver)
                return this.ProcessField(this.InvokeWithArguments(arguments, fieldInfo.Lambda));

            return this.ProcessField(fieldInfo.Lambda.Compile().DynamicInvoke(new object[] { parent }));
        }

        private object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression)
        {
            var argumentValues = this.valueResolver.FetchArgumentValues(expression, arguments);

            return expression.Compile().DynamicInvoke(argumentValues);
        }
    }
}