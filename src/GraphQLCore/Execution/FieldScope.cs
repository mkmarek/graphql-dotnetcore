namespace GraphQLCore.Execution
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Type;
    using Type.Complex;
    using Type.Directives;
    using Type.Scalar;
    using Utils;

    public class FieldScope
    {
        public IList<GraphQLException> Errors { get; }

        private static Guid INVALID_RESULT = Guid.NewGuid();

        private List<GraphQLArgument> arguments;
        private ExecutionContext context;
        private object parent;
        private GraphQLComplexType type;
        private IEnumerable<object> path;

        public FieldScope(
            ExecutionContext context,
            GraphQLComplexType type,
            object parent,
            IEnumerable<object> parentPath = null,
            IList<GraphQLException> parentErrors = null)
        {
            this.type = type;
            this.parent = parent;
            this.arguments = new List<GraphQLArgument>();
            this.context = context;
            this.path = parentPath ?? new List<object>();
            this.Errors = parentErrors ?? new List<GraphQLException>();
        }

        public async Task<object> CompleteValue(
            object input,
            Type inputType,
            GraphQLFieldSelection selection,
            IList<GraphQLArgument> arguments,
            IEnumerable<object> path = null)
        {
            if (path == null)
                path = this.path;

            if (inputType == null)
                return null;

            var resolvers = new List<Func<object, Type, GraphQLFieldSelection, IEnumerable<object>, Task<object>>>()
            {
                this.TryResolveNonNull,
                this.TryResolveNull,
                this.TryResolveUnion,
                this.TryResolveObjectType,
                this.TryResolveCollection,
                this.TryResolveGraphQLObjectType,
                this.TryResolveEnum
            };

            foreach (var resolver in resolvers)
            {
                var result = await resolver(input, inputType, selection, path);

                if (!INVALID_RESULT.Equals(result))
                {
                    return result;
                }
            }

            return input;
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

        public async Task<ExpandoObject> GetObject(
            Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            await Task.WhenAll(fields.Select(
                  field => this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value)));

            return result;
        }

        public async Task<ExpandoObject> GetObjectSynchronously(
            Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
            {
                await this.AddFieldsFromSelectionToResultDictionary(dictionary, field.Key, field.Value);
            }

            return result;
        }

        public async Task<object> InvokeWithArguments(
            IList<GraphQLArgument> arguments, LambdaExpression expression, object parent = null)
        {
            var argumentValues = this.FetchArgumentValues(expression, arguments, parent);

            var result = expression.Compile().DynamicInvoke(argumentValues);

            return await this.HandleAsyncTaskIfAsync(result);
        }

        public object InvokeWithArgumentsSync(
            IList<GraphQLArgument> arguments, LambdaExpression expression, object parent = null)
        {
            return this.InvokeWithArguments(arguments, expression, parent).Result;
        }

        private async Task<object> HandleAsyncTaskIfAsync(object result)
        {
            if (
                result is Task &&
                (!result.GetType().GetTypeInfo().GetGenericArguments().Any() ||
                    result.GetType().GetTypeInfo().GetGenericArguments()?.FirstOrDefault()?.Name == "VoidTaskResult"))
            {
                await (Task)result;

                return null;
            }

            if (result is Task)
            {
                Task r = (Task)result;

                return await Task.Run(() => ((dynamic)result).GetAwaiter().GetResult());
            }

            return await Task.FromResult(result);
        }

        private async Task AddFieldsFromSelectionToResultDictionary(
            IDictionary<string, object> dictionary, string fieldName, IList<GraphQLFieldSelection> fieldSelections)
        {
            await Task.WhenAll(fieldSelections.Select(
                selection => this.AddToResultDictionaryIfNotAlreadyPresent(dictionary, fieldName, selection)));

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

        private async Task AddToResultDictionaryIfNotAlreadyPresent(
            IDictionary<string, object> dictionary,
            string fieldName,
            GraphQLFieldSelection selection)
        {
            var fieldData = await this.GetDefinitionAndExecuteField(this.type, selection, dictionary);

            lock (dictionary)
            {
                if (!dictionary.ContainsKey(fieldName))
                {
                    dictionary.Add(fieldName, fieldData);
                }
            }
        }

        private async Task<object> CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments, IEnumerable<object> path)
        {
            var result = new List<object>();
            var index = 0;

            foreach (var element in input)
            {
                result.Add(await this.CompleteValue(element, element?.GetType(), selection, arguments, path.Append(index)));

                index++;
            }

            return result;
        }

        private async Task<object> CompleteObjectType(
            GraphQLObjectType input,
            GraphQLFieldSelection selection,
            IList<GraphQLArgument> arguments,
            object parentObject,
            IEnumerable<object> path)
        {
            var aliasOrName = selection.Alias?.Value ?? selection.Name.Value;

            var scope = new FieldScope(this.context, input, parentObject, path.Append(aliasOrName), this.Errors)
            {
                arguments = arguments.ToList()
            };

            return await this.TryGetObject(scope, input, selection, path);
        }

        private async Task<ExpandoObject> TryGetObject(FieldScope scope, GraphQLObjectType input, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            try
            {
                return await scope.GetObject(this.context.FieldCollector.CollectFields(input, selection.SelectionSet));
            }
            catch (GraphQLResolveException)
            {
                return null;
            }
        }

        private List<GraphQLArgument> GetArgumentsFromDirectiveDefinition(GraphQLDirective directive,
            GraphQLDirectiveType directiveInfo)
        {
            if (directiveInfo == null || directiveInfo.Arguments?.Count == 0)
                return null;

            return this.MergeArgumentsWithDefault(directive.Arguments, directiveInfo.Arguments);
        }

        private List<GraphQLArgument> GetArgumentsFromSelection(GraphQLFieldSelection selection, GraphQLFieldInfo fieldInfo)
        {
            if (fieldInfo == null || fieldInfo.Arguments?.Count == 0)
                return null;

            return this.MergeArgumentsWithDefault(selection.Arguments, fieldInfo.Arguments);
        }

        private List<GraphQLArgument> MergeArgumentsWithDefault(IEnumerable<GraphQLArgument> arguments,
            IDictionary<string, GraphQLObjectTypeArgumentInfo> argumentInfos)
        {
            var combinedArguments = arguments.ToDictionary(e => e.Name.Value, e => e);
            var schemaRepository = this.context.SchemaRepository;

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

        private async Task<object> GetDefinitionAndExecuteField(
            GraphQLComplexType type,
            GraphQLFieldSelection selection,
            IDictionary<string, object> dictionary)
        {
            var fieldInfo = this.GetFieldInfo(type, selection);
            var arguments = this.GetArgumentsFromSelection(selection, fieldInfo)?.ToList() ?? new List<GraphQLArgument>();
            var directivesToUse = selection.Directives;

            Func<Task<object>> fieldResolver = async () =>
            {
                var fieldResult = await this.TryResolveField(selection, fieldInfo, arguments, this.parent);

                await this.PublishToEventChannel(fieldInfo, fieldResult);

                return fieldResult;
            };

            var result = await this.ApplyDirectivesToResult(selection, dictionary, fieldResolver);
            var resultType = this.GetResultType(type, fieldInfo, result);

            return await this.CompleteValue(result, resultType, selection, arguments, this.path);
        }

        private async Task PublishToEventChannel(GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
            if (!string.IsNullOrEmpty(fieldInfo?.Channel) && this.context.OperationType == OperationType.Mutation)
            {
                await this.context.Schema?.SubscriptionType?.EventBus?.Publish(result, fieldInfo.Channel);
            }
        }

        private async Task<object> ApplyDirectivesToResult(GraphQLFieldSelection selection, IDictionary<string, object> dictionary, Func<Task<object>> fieldResolver)
        {
            foreach (var directive in selection.Directives)
            {
                var directiveType = this.context.SchemaRepository.GetDirective(directive.Name.Value);

                if (directiveType != null && directiveType.Locations.Any(l => l == DirectiveLocation.FIELD))
                {
                    var newResolver = directiveType.GetResolver(fieldResolver, (ExpandoObject)dictionary);

                    fieldResolver = async () => await this.InvokeWithArguments(
                        this.GetArgumentsFromDirectiveDefinition(directive, directiveType),
                        newResolver);
                }
            }

            return await fieldResolver();
        }

        private Type GetResultType(
            GraphQLComplexType type, GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
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

        private async Task<object> TryResolveField(
            GraphQLFieldSelection selection, GraphQLObjectTypeFieldInfo fieldInfo, IList<GraphQLArgument> arguments, object parent)
        {
            try
            {
                return await this.ResolveField(fieldInfo, arguments, parent);
            }
            catch (TargetInvocationException ex)
            {
                var aliasOrName = selection.Alias?.Value ?? selection.Name.Value;
                var orderedPath = this.ReorderPath(this.path);

                var exception = new GraphQLException(ex.InnerException);
                var locatedException = GraphQLException.LocateException(exception, new[] { selection }, orderedPath.Append(aliasOrName));
                this.Errors.Add(locatedException);

                return await this.CompleteValue(null, fieldInfo.SystemType, selection, arguments, this.path);
            }
        }

        private IEnumerable<object> ReorderPath(IEnumerable<object> path)
        {
            var index = 0;

            // Reorder elements so array indexes are placed after array name
            return path.OrderBy(e =>
            {
                var value = index;

                if (e is int)
                    value += 2;
                index++;

                return value;
            });
        }

        private async Task<object> ResolveField(
            GraphQLObjectTypeFieldInfo fieldInfo, IList<GraphQLArgument> arguments, object parent)
        {
            if (fieldInfo == null)
                return null;

            if (fieldInfo.IsResolver)
            {
                var resolverResult = await this.InvokeWithArguments(arguments, fieldInfo.Lambda);

                return this.ProcessField(resolverResult);
            }

            var accessorResult = fieldInfo.Lambda.Compile().DynamicInvoke(new object[] { parent });
            accessorResult = await this.HandleAsyncTaskIfAsync(accessorResult);

            return this.ProcessField(accessorResult);
        }

        private async Task<object> TryResolveEnum(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            return await Task.Run(() =>
            {
                if (ReflectionUtilities.IsEnum(inputType))
                {
                    input = input.ToString();

                    return input;
                }

                return INVALID_RESULT;
            });
        }

        private async Task<object> TryResolveGraphQLObjectType(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            var schemaValue = this.context.SchemaRepository.GetSchemaTypeFor(inputType);
            if (schemaValue is GraphQLObjectType)
            {
                input = await this.CompleteObjectType((GraphQLObjectType)schemaValue, selection, this.arguments, input, path);

                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveCollection(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            if (ReflectionUtilities.IsCollection(inputType))
            {
                input = await this.CompleteCollectionType((IEnumerable)input, selection, this.arguments, path);

                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveObjectType(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            if (ReflectionUtilities.IsDescendant(inputType, typeof(GraphQLObjectType)))
            {
                input = await this.CompleteObjectType((GraphQLObjectType)input, selection, this.arguments, this.parent, path);
                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveUnion(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            if (ReflectionUtilities.IsDescendant(inputType, typeof(GraphQLUnionType)))
            {
                var unionSchemaType = this.context.SchemaRepository.GetSchemaTypeFor(inputType) as GraphQLUnionType;
                input = await this.CompleteValue(input, unionSchemaType.ResolveType(input), selection, this.arguments, path);

                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveNonNull(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            var underlyingType = NonNullable.GetUnderlyingType(inputType);
            if (underlyingType != null)
            {
                input = (input as INonNullable)?.GetValue() ?? input;

                input = await this.CompleteValue(input, underlyingType, selection, this.arguments, path);

                if (input == null)
                    throw new GraphQLResolveException($"Cannot return null for non-nullable field {selection.Name.Value}.");

                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveNull(object input, Type inputType, GraphQLFieldSelection selection, IEnumerable<object> path)
        {
            return await Task.Run(() =>
            {
                if (input == null)
                    return (object)null;

                return INVALID_RESULT;
            });
        }
    }
}