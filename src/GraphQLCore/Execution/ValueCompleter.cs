namespace GraphQLCore.Execution
{
    using Exceptions;
    using Language.AST;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Threading.Tasks;
    using Type;
    using Type.Complex;
    using Utils;

    public class ValueCompleter : IValueCompleter
    {
        private static Guid INVALID_RESULT = Guid.NewGuid();

        private ExecutionContext context;

        public ValueCompleter(ExecutionContext context)
        {
            this.context = context;
        }

        public async Task<object> CompleteValue(ExecutedField field, object result)
        {
            var value = ValueToComplete.Create(
                result,
                this.GetResultType(field.FieldInfo, result),
                field.Selection,
                field.Path,
                field.Errors);

            return await this.CompleteValue(value);
        }

        private Type GetResultType(GraphQLObjectTypeFieldInfo fieldInfo, object result)
        {
            return fieldInfo?.SystemType ?? result?.GetType();
        }

        private async Task<object> CompleteValue(ValueToComplete value)
        {
            if (value.InputType == null)
                return null;

            var resolvers = new List<Func<ValueToComplete, Task<object>>>()
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
                var result = await resolver(value);

                if (!INVALID_RESULT.Equals(result))
                {
                    return result;
                }
            }

            return value.Input;
        }

        private async Task<object> TryResolveEnum(ValueToComplete value)
        {
            return await Task.Run(() =>
            {
                if (ReflectionUtilities.IsEnum(value.InputType))
                {
                    return value.Input.ToString() as object;
                }

                return INVALID_RESULT;
            });
        }

        private async Task<object> TryResolveGraphQLObjectType(ValueToComplete value)
        {
            var schemaValue = this.context.SchemaRepository.GetSchemaTypeFor(value.InputType);
            if (schemaValue is GraphQLObjectType)
            {
                return await this.CompleteObjectType((GraphQLObjectType)schemaValue, value.Selection, null, value.Input, value.Path, value.Errors);
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveCollection(ValueToComplete value)
        {
            if (ReflectionUtilities.IsCollection(value.InputType))
            {
                return await this.CompleteCollectionType((IEnumerable)value.Input, value.Selection, value.Path, value.Errors);
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveObjectType(ValueToComplete value)
        {
            if (ReflectionUtilities.IsDescendant(value.InputType, typeof(GraphQLObjectType)))
            {
                return await this.CompleteObjectType((GraphQLObjectType)value.Input, value.Selection, null, value.Input, value.Path, value.Errors);
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveUnion(ValueToComplete value)
        {
            if (ReflectionUtilities.IsDescendant(value.InputType, typeof(GraphQLUnionType)))
            {
                var unionSchemaType = this.context.SchemaRepository.GetSchemaTypeFor(value.InputType) as GraphQLUnionType;
                var newValue = ValueToComplete.Create(
                    value.Input,
                    unionSchemaType.ResolveType(value.Input),
                    value.Selection,
                    value.Path,
                    value.Errors);

                return await this.CompleteValue(newValue);
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveNonNull(ValueToComplete value)
        {
            var underlyingType = NonNullable.GetUnderlyingType(value.InputType);
            if (underlyingType != null)
            {
                var newValue = ValueToComplete.Create(
                    (value.Input as INonNullable)?.GetValue() ?? value.Input,
                    underlyingType,
                    value.Selection,
                    value.Path,
                    value.Errors);

                var input = await this.CompleteValue(newValue);

                if (input == null)
                    throw new GraphQLResolveException($"Cannot return null for non-nullable field {value.Selection.Name.Value}.");

                return input;
            }

            return INVALID_RESULT;
        }

        private async Task<object> TryResolveNull(ValueToComplete value)
        {
            return await Task.Run(() =>
            {
                if (value.Input == null)
                    return (object)null;

                return INVALID_RESULT;
            });
        }

        private async Task<object> CompleteObjectType(
            GraphQLObjectType input,
            GraphQLFieldSelection selection,
            IList<GraphQLArgument> arguments,
            object parentObject,
            IEnumerable<object> path,
            IList<GraphQLException> errors)
        {
            var field = new ExecutedField()
            {
                Selection = selection,
                Arguments = arguments,
                Path = path,
                Errors = errors
            };

            var scope = new FieldScope(this.context, input, field, parentObject);

            return await this.TryGetObject(scope, input, field);
        }

        private async Task<ExpandoObject> TryGetObject(FieldScope scope, GraphQLObjectType input, ExecutedField field)
        {
            try
            {
                return await scope.GetObject(this.context.FieldCollector.CollectFields(input, field.Selection.SelectionSet, scope));
            }
            catch (GraphQLResolveException)
            {
                return null;
            }
        }

        private async Task<object> CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IEnumerable<object> path, IList<GraphQLException> errors)
        {
            var result = new List<object>();
            var index = 0;

            foreach (var element in input)
            {
                var value = ValueToComplete.CreateForCollection(element, element?.GetType(), selection, path, index, errors);
                result.Add(await this.CompleteValue(value));

                index++;
            }

            return result;
        }
    }
}
