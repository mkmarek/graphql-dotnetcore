namespace GraphQLCore.Execution
{
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Scalar;
    using Utils;

    public class FieldScope
    {
        private List<GraphQLArgument> arguments;
        private ExecutionContext context;
        private object parent;
        private GraphQLComplexType type;

        public FieldScope(
            ExecutionContext context,
            GraphQLComplexType type,
            object parent)
        {
            this.type = type;
            this.parent = parent;
            this.arguments = new List<GraphQLArgument>();
            this.context = context;
        }

        public object CompleteValue(
            object input,
            Type inputType,
            GraphQLFieldSelection selection,
            IList<GraphQLArgument> arguments)
        {
            if (input == null || inputType == null)
                return null;

            var result = input;

            var resolved =
                  this.TryResolveUnion(ref result, inputType, selection)
               || this.TryResolveNonNull(ref result, inputType, selection)
               || this.TryResolveObjectType(ref result, inputType, selection)
               || this.TryResolveCollection(ref result, inputType, selection)
               || this.TryResolveGraphQLObjectType(ref result, inputType, selection)
               || this.TryResolveEnum(ref result, inputType);

            return resolved ? result : input;
        }

        public object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments, object parent)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => this.GetValueForArgument(arguments, e, parent))
                .ToArray();
        }

        public object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName, GraphQLInputType type)
        {
            var argument = arguments.SingleOrDefault(e => e.Name.Value == argumentName);

            if (argument == null)
                return null;

            return type.GetFromAst(argument.Value, this.context.SchemaRepository).Value;
        }

        public dynamic GetObject(Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value);

            return result;
        }

        public object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression, object parent = null)
        {
            var argumentValues = this.FetchArgumentValues(expression, arguments, parent);

            return expression.Compile().DynamicInvoke(argumentValues);
        }

        private void AddFieldsFromSelectionToResultDictionary(
            IDictionary<string, object> dictionary, string fieldName, IList<GraphQLFieldSelection> fieldSelections)
        {
            foreach (var selection in fieldSelections)
                this.AddToResultDictionaryIfNotAlreadyPresent(dictionary, fieldName, selection);

            foreach (var selection in fieldSelections)
                this.ApplyDirectives(dictionary, fieldName, selection);
        }

        private object GetValueForArgument(IList<GraphQLArgument> arguments, ParameterExpression e, object parent)
        {
            if (this.IsContextType(e))
                return this.CreateContextObject(e.Type, parent);

            return ReflectionUtilities.ChangeValueType(
                this.GetArgumentValue(
                    arguments,
                    e.Name,
                    this.context.SchemaRepository.GetSchemaInputTypeFor(e.Type)),
                    e.Type);
        }

        private bool IsContextType(ParameterExpression e)
        {
            var contextType = typeof(IContext<>);

            return e.Type.GetTypeInfo().IsGenericType && e.Type.GetGenericTypeDefinition() == contextType;
        }

        private object CreateContextObject(Type type, object parent)
        {
            var genericArgument = type.GetTypeInfo()
                .GetGenericArguments()
                .Single();

            var fieldContextType = typeof(FieldContext<>)
                .MakeGenericType(genericArgument);

            return Activator.CreateInstance(fieldContextType, parent ?? this.parent);
        }

        private void ApplyDirectives(
            IDictionary<string, object> dictionary,
            string fieldName,
            GraphQLFieldSelection selection)
        {
            if (dictionary.ContainsKey(fieldName))
            {
                foreach (var directive in selection.Directives)
                {
                    var directiveType = this.context.SchemaRepository.GetDirective(directive.Name.Value);

                    if (!directiveType.PostExecutionIncludeFieldIntoResult(
                            directive, this.context.SchemaRepository, dictionary[fieldName], (ExpandoObject)dictionary))
                            dictionary.Remove(fieldName);
                }
            }
        }

        private void AddToResultDictionaryIfNotAlreadyPresent(
            IDictionary<string, object> dictionary,
            string fieldName,
            GraphQLFieldSelection selection)
        {
            if (!dictionary.ContainsKey(fieldName))
            {
                var fieldData = this.GetDefinitionAndExecuteField(this.type, selection, dictionary);

                dictionary.Add(fieldName, fieldData);
            }
        }

        private object CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            var result = new List<object>();
            foreach (var element in input)
                result.Add(this.CompleteValue(element, element?.GetType(), selection, arguments));

            return result;
        }

        private object CompleteObjectType(
            GraphQLObjectType input,
            GraphQLFieldSelection selection,
            IList<GraphQLArgument> arguments,
            object parentObject)
        {
            var scope = new FieldScope(this.context, input, parentObject)
            {
                arguments = arguments.ToList()
            };

            return scope.GetObject(this.context.FieldCollector.CollectFields(input, selection.SelectionSet));
        }

        private List<GraphQLArgument> GetArgumentsFromSelection(GraphQLFieldSelection selection)
        {
            var arguments = this.arguments.ToList();

            arguments.RemoveAll(e => selection.Arguments.Any(arg => arg.Name.Value.Equals(e.Name.Value)));
            arguments.AddRange(selection.Arguments);

            return arguments;
        }

        private object GetDefinitionAndExecuteField(
            GraphQLComplexType type,
            GraphQLFieldSelection selection,
            IDictionary<string, object> dictionary)
        {
            var arguments = this.GetArgumentsFromSelection(selection);
            var fieldInfo = this.GetFieldInfo(type, selection);
            var directivesToUse = selection.Directives;

            var result = this.ResolveField(fieldInfo, arguments, this.parent);

            this.PublishToEventChannel(fieldInfo, result);

            result = this.ApplyDirectivesToResult(selection, dictionary, result);
            var resultType = this.GetResultType(type, fieldInfo, result);

            return this.CompleteValue(result, resultType, selection, arguments);
        }

        private void PublishToEventChannel(GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
            if (!string.IsNullOrEmpty(fieldInfo?.Channel) && this.context.OperationType == OperationType.Mutation)
            {
                this.context.Schema?.SubscriptionType?.EventBus?.Publish(result, fieldInfo.Channel);
            }
        }

        private object ApplyDirectivesToResult(GraphQLFieldSelection selection, IDictionary<string, object> dictionary, object result)
        {
            foreach (var directive in selection.Directives)
            {
                var directiveType = this.context.SchemaRepository.GetDirective(directive.Name.Value);

                if (directiveType != null && directiveType.Locations.Any(l => l == DirectiveLocation.FIELD))
                {
                    result = this.InvokeWithArguments(
                        directive.Arguments.ToList(),
                        directiveType.GetResolver(result, (ExpandoObject)dictionary));
                }
            }

            return result;
        }

        private Type GetResultType(
            GraphQLComplexType type, GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
            if (type is GraphQLSubscriptionType)
                return ((GraphQLSubscriptionTypeFieldInfo)fieldInfo).SubscriptionReturnType;

            return fieldInfo?.SystemType ?? result?.GetType();
        }

        private GraphQLObjectTypeFieldInfo GetFieldInfo(GraphQLComplexType type, GraphQLFieldSelection selection)
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

            if (input is ID)
                return (string)(ID)input;

            if (ReflectionUtilities.IsEnum(input.GetType()))
                return input.ToString();

            return input;
        }

        private object ResolveField(
            GraphQLObjectTypeFieldInfo fieldInfo, IList<GraphQLArgument> arguments, object parent)
        {
            if (fieldInfo == null)
                return null;

            if (fieldInfo.IsResolver)
                return this.ProcessField(this.InvokeWithArguments(arguments, fieldInfo.Lambda));

            return this.ProcessField(fieldInfo.Lambda.Compile().DynamicInvoke(new object[] { parent }));
        }

        private bool TryResolveEnum(ref object input, Type inputType)
        {
            if (ReflectionUtilities.IsEnum(inputType))
            {
                input = input.ToString();

                return true;
            }

            return false;
        }

        private bool TryResolveGraphQLObjectType(ref object input, Type inputType, GraphQLFieldSelection selection)
        {
            var schemaValue = this.context.SchemaRepository.GetSchemaTypeFor(inputType);
            if (schemaValue is GraphQLObjectType)
            {
                input = this.CompleteObjectType((GraphQLObjectType)schemaValue, selection, this.arguments, input);

                return true;
            }

            return false;
        }

        private bool TryResolveCollection(ref object input, Type inputType, GraphQLFieldSelection selection)
        {
            if (ReflectionUtilities.IsCollection(inputType))
            {
                input = this.CompleteCollectionType((IEnumerable)input, selection, this.arguments);

                return true;
            }

            return false;
        }

        private bool TryResolveObjectType(ref object input, Type inputType, GraphQLFieldSelection selection)
        {
            if (ReflectionUtilities.IsDescendant(inputType, typeof(GraphQLObjectType)))
            {
                input = this.CompleteObjectType((GraphQLObjectType)input, selection, this.arguments, this.parent);
                return true;
            }

            return false;
        }

        private bool TryResolveUnion(ref object input, Type inputType, GraphQLFieldSelection selection)
        {
            if (ReflectionUtilities.IsDescendant(inputType, typeof(GraphQLUnionType)))
            {
                var unionSchemaType = this.context.SchemaRepository.GetSchemaTypeFor(inputType) as GraphQLUnionType;
                input = this.CompleteValue(input, unionSchemaType.ResolveType(input), selection, this.arguments);

                return true;
            }

            return false;
        }

        private bool TryResolveNonNull(ref object input, Type inputType, GraphQLFieldSelection selection)
        {
            var underlyingType = NonNullable.GetUnderlyingType(inputType);
            if (underlyingType != null)
            {
                input = this.CompleteValue(((INonNullable)input).GetValue(), underlyingType, selection, this.arguments);

                return true;
            }

            return false;
        }
    }
}