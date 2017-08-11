namespace GraphQLCore.Execution
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Scalar;
    using Utils;

    public class FieldScope
    {
        public ExecutionContext Context { get; }
        public IList<GraphQLException> Errors { get; }
        public IEnumerable<object> Path { get; }

        private object Parent { get; }
        private GraphQLComplexType Type { get; }

        public FieldScope(
            ExecutionContext context,
            GraphQLComplexType type)
        {
            this.Context = context;

            this.Type = type;
            this.Path = new List<object>();
            this.Errors = new List<GraphQLException>();
        }

        public FieldScope(
            ExecutionContext context,
            GraphQLComplexType type,
            ExecutedField parentField,
            object parent)
            : this(context, type)
        {
            this.Errors = parentField?.Errors ?? new List<GraphQLException>();
            this.Path = parentField?.Path ?? new List<object>();
            this.Parent = parent;
        }

        public async Task<dynamic> GetSingleField(
            GraphQLFieldSelection field)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;
            return await this.GetDefinitionAndExecuteField(this.Type, field, dictionary);
        }

        public async Task<ExpandoObject> GetObject(
            Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var dictionary = new ResultDictionary();

            await Task.WhenAll(fields.Select(
                (field, index) =>
                this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value, new[] { index })));

            return dictionary.GetOrdered();
        }

        public async Task<ExpandoObject> GetObjectSynchronously(
            Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var dictionary = new ResultDictionary();

            var index = 0;
            foreach (var field in fields)
            {
                await this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value, new[] { index });
                index++;
            }

            return dictionary.GetOrdered();
        }

        public async Task<object> InvokeWithArguments(
            IList<GraphQLArgument> arguments, LambdaExpression expression, object parent = null)
        {
            var argumentValues = this.Context.ArgumentFetcher.FetchArgumentValues(expression, arguments, parent);
            var result = expression.Compile().DynamicInvoke(argumentValues);

            return await AsyncUtils.HandleAsyncTaskIfAsync(result);
        }

        public object InvokeWithArgumentsSync(
            IList<GraphQLArgument> arguments, LambdaExpression expression, object parent = null)
        {
            return this.InvokeWithArguments(arguments, expression, parent).Result;
        }

        private async Task AddFieldsFromSelectionToResultDictionary(
            ResultDictionary dictionary, string fieldName, IList<GraphQLFieldSelection> fieldSelections, IEnumerable<int> parentIndex)
        {
            await Task.WhenAll(fieldSelections.Select(
                (selection, index) => this.AddToResultDictionaryIfNotAlreadyPresent(dictionary, fieldName, selection, parentIndex.Append(index))));

            foreach (var selection in fieldSelections)
                this.ApplyDirectives(dictionary, fieldName, selection);
        }

        private void ApplyDirectives(
            ResultDictionary dictionary,
            string fieldName,
            GraphQLFieldSelection selection)
        {
            if (dictionary.ContainsKey(fieldName))
            {
                foreach (var directive in selection.Directives)
                {
                    var directiveType = this.Context.SchemaRepository.GetDirective(directive.Name.Value);

                    if (!directiveType.PostExecutionIncludeFieldIntoResult(
                        directive, this.Context.SchemaRepository, dictionary[fieldName], (ExpandoObject)dictionary))
                        dictionary.Remove(fieldName);
                }
            }
        }

        private async Task AddToResultDictionaryIfNotAlreadyPresent(
            ResultDictionary dictionary,
            string fieldName,
            GraphQLFieldSelection selection,
            IEnumerable<int> index)
        {
            var fieldData = await this.GetDefinitionAndExecuteField(this.Type, selection, dictionary);

            lock (dictionary)
            {
                if (!dictionary.ContainsKey(fieldName))
                {
                    dictionary.Insert(index.ToArray(), fieldName, fieldData);
                }
            }
        }

        private ExecutedField GetExecutedField(GraphQLFieldSelection selection, GraphQLObjectTypeFieldInfo fieldInfo)
        {
            var arguments = selection.GetArgumentsWithDefaultValues(fieldInfo, this.Context.SchemaRepository);

            return new ExecutedField()
            {
                Arguments = arguments,
                FieldInfo = fieldInfo,
                Selection = selection,
                Path = this.Path.Append(selection.GetPathName()),
                Errors = this.Errors
            };
        }

        private async Task<object> GetDefinitionAndExecuteField(
            GraphQLComplexType type,
            GraphQLFieldSelection selection,
            ResultDictionary dictionary)
        {
            var fieldInfo = this.GetFieldInfo(type, selection);
            if (fieldInfo == null)
                return null;

            var field = this.GetExecutedField(selection, fieldInfo);

            Func<Task<object>> fieldResolver = async () =>
            {
                var fieldResult = await this.TryResolveField(field);
                await this.PublishToEventChannel(fieldInfo, fieldResult);

                return fieldResult;
            };

            var result = await this.ApplyDirectivesToResult(selection, dictionary, fieldResolver);

            return await this.Context.ValueCompleter.CompleteValue(field, result);
        }

        private async Task PublishToEventChannel(GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
            if (!string.IsNullOrEmpty(fieldInfo?.Channel) && this.Context.OperationType == OperationType.Mutation)
            {
                await this.Context.Schema?.SubscriptionType?.EventBus?.Publish(result, fieldInfo.Channel);
            }
        }

        private async Task<object> ApplyDirectivesToResult(GraphQLFieldSelection selection, ResultDictionary dictionary, Func<Task<object>> fieldResolver)
        {
            foreach (var directive in selection.Directives)
            {
                var directiveType = this.Context.SchemaRepository.GetDirective(directive.Name.Value);

                if (directiveType != null && directiveType.Locations.Any(l => l == DirectiveLocation.FIELD))
                {
                    var newResolver = directiveType.GetResolver(fieldResolver, (ExpandoObject)dictionary);

                    fieldResolver = async () => await this.InvokeWithArguments(
                        directive.GetArgumentsWithDefaultValues(directiveType, this.Context.SchemaRepository),
                        newResolver);
                }
            }

            return await fieldResolver();
        }

        private GraphQLObjectTypeFieldInfo GetFieldInfo(GraphQLComplexType type, GraphQLFieldSelection selection)
        {
            return type.GetFieldInfo(selection.Name.Value);
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

        private async Task<object> TryResolveField(ExecutedField field)
        {
            try
            {
                var fieldExpression = field.GetExpression(this.Context.SchemaRepository, this.Parent);
                return await this.ResolveField(fieldExpression);
            }
            catch (TargetInvocationException ex)
            {
                var exception = new GraphQLException(ex.InnerException);
                var locatedException = GraphQLException.LocateException(exception, new[] { field.Selection }, field.Path);
                this.Errors.Add(locatedException);

                return await this.Context.ValueCompleter.CompleteValue(field, null);
            }
        }

        private async Task<object> ResolveField(IFieldExpression field)
        {
            var result = await field.GetResult();

            return this.ProcessField(result);
        }
    }
}