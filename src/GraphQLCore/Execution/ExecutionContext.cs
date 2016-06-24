namespace GraphQLCore.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Language.AST;
    using Type;
    using System.Dynamic;
    using System.Linq;
    using Type.Introspection;
    using Utils;
    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument AST;
        private GraphQLSchema GraphQLSchema;
        private GraphQLOperationDefinition Operation;
        private Dictionary<int, object> ResolvedObjectCache;
        private Dictionary<string, GraphQLFragmentDefinition> Fragments;

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.GraphQLSchema = graphQLSchema;
            this.AST = ast;
            this.ResolvedObjectCache = new Dictionary<int, object>();
            this.Fragments = new Dictionary<string, GraphQLFragmentDefinition>();
        }

        public void Dispose()
        {
        }

        public dynamic Execute()
        {
            foreach (var definition in this.AST.Definitions)
                this.ResolveDefinition(definition);

            if (this.Operation == null)
                throw new Exception("Must provide an operation.");

            var type = this.GetOperationRootType();
            var fields = this.CollectFields(type, this.Operation.SelectionSet);

            return this.ExecuteFields(type, fields, new List<GraphQLArgument>());
        }

        private dynamic ExecuteFields(GraphQLObjectType type, Dictionary<string, IList<GraphQLFieldSelection>> fields, IList<GraphQLArgument> arguments)
        {
            var result = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)result;

            foreach (var field in fields)
                foreach (var selection in field.Value)
                    dictionary.Add(
                        field.Key,
                        this.ExecuteField(
                            GetFieldDefinition(type, selection),
                            selection, arguments));

            return result;
        }

        private Func<IList<GraphQLArgument>, object> GetFieldDefinition(GraphQLObjectType type, GraphQLFieldSelection selection)
        {
            if (selection.Name.Value == GraphQLSchema.__Schema.Name.ToLower())
                return (args) => GraphQLSchema.__Schema;

            return (args) => type.ResolveField(selection, this.ResolvedObjectCache, args);
        }

        private object ExecuteField(Func<IList<GraphQLArgument>, object> fieldResolver, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            var args = arguments.ToList();
            args.RemoveAll(e => selection.Arguments.Any(arg => arg.Name.Value.Equals(e.Name.Value)));
            args.AddRange(selection.Arguments);

            return this.CompleteValue(fieldResolver(args), selection, args);
        }

        private object CompleteValue(object input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            if (input == null)
                return null;

            if (input is GraphQLObjectType)
                return this.CompleteObjectType((GraphQLObjectType)input, selection, arguments);

            if (ReflectionUtilities.IsCollection(input.GetType()))
                return this.CompleteCollectionType((IEnumerable)input, selection, arguments);

            return input;
        }

        private object CompleteCollectionType(IEnumerable input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            var result = new List<object>();
            foreach (var element in input)
                result.Add(this.CompleteValue(element, selection, arguments));

            return result;
        }

        private object CompleteObjectType(GraphQLObjectType input, GraphQLFieldSelection selection, IList<GraphQLArgument> arguments)
        {
            return this.ExecuteFields(input, this.CollectFields(input, selection.SelectionSet), arguments);
        }

        private Dictionary<string, IList<GraphQLFieldSelection>> CollectFields(GraphQLObjectType runtimeType, GraphQLSelectionSet selectionSet)
        {
            var fields = new Dictionary<string, IList<GraphQLFieldSelection>>();

            foreach (var selection in selectionSet.Selections)
                this.CollectFieldsInSelection(runtimeType, selection, fields);

            return fields;
        }

        private void CollectFieldsInSelection(GraphQLObjectType runtimeType, ASTNode selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            switch (selection.Kind)
            {
                case ASTNodeKind.Field: this.CollectField((GraphQLFieldSelection)selection, fields); break;
                case ASTNodeKind.FragmentSpread: this.CollectFragmentSpreadFields(runtimeType, (GraphQLFragmentSpread)selection, fields); break;
                case ASTNodeKind.InlineFragment: this.CollectFragmentFields(runtimeType, (GraphQLInlineFragment)selection, fields); break;
            }
        }

        private void CollectFragmentSpreadFields(GraphQLObjectType runtimeType, GraphQLFragmentSpread fragmentSpread, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            var fragment = this.Fragments[fragmentSpread.Name.Value];
            CollectFragmentFields(runtimeType, fragment, fields);
        }

        private void CollectFragmentFields(GraphQLObjectType runtimeType, GraphQLInlineFragment fragment, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(fragment.Directives))
                return;

            if (!this.DoesFragmentConditionMatch(runtimeType, fragment))
                return;

            this.CollectFields(runtimeType, fragment.SelectionSet)
                .ToList().ForEach(e => fields.Add(e.Key, e.Value));
        }

        private bool DoesFragmentConditionMatch(GraphQLObjectType runtimeType, GraphQLInlineFragment fragment)
        {
            if (fragment.TypeCondition == null)
                return true;

            if (fragment.TypeCondition.Name.Value == runtimeType.Name)
                return true;

            return false;
        }

        private void CollectField(GraphQLFieldSelection selection, Dictionary<string, IList<GraphQLFieldSelection>> fields)
        {
            if (!this.ShouldIncludeNode(selection.Directives))
                return;

            var entryKey = this.GetFieldEntryKey(selection);

            if (!fields.ContainsKey(entryKey))
                fields.Add(entryKey, new List<GraphQLFieldSelection>());

            fields[entryKey].Add(selection);
        }

        private bool ShouldIncludeNode(IEnumerable<GraphQLDirective> directives)
        {
            var skipAST = directives?.FirstOrDefault(e => e.Name.Value == "skip");
            if (skipAST != null && GetArgumentValue(skipAST.Arguments, "if").Equals(true))
                return false;

            var includeAST = directives?.FirstOrDefault(e => e.Name.Value == "include");
            if (includeAST != null && GetArgumentValue(includeAST.Arguments, "if").Equals(false))
                return false;

            return true;
        }

        public static object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName)
        {
            var value = arguments.SingleOrDefault(e => e.Name.Value == argumentName).Value;

            return GetValue(value);
        }

        private static object GetValue(GraphQLValue value)
        {
            switch (value.Kind)
            {
                case ASTNodeKind.BooleanValue: return ((GraphQLValue<bool>)value).Value;
                case ASTNodeKind.IntValue: return ((GraphQLValue<int>)value).Value;
                case ASTNodeKind.FloatValue: return ((GraphQLValue<float>)value).Value;
                case ASTNodeKind.StringValue: return ((GraphQLValue<string>)value).Value;
                case ASTNodeKind.ListValue: return GetListValue(value);
            }

            throw new NotImplementedException();
        }

        private static IEnumerable GetListValue(GraphQLValue value)
        {
            IList output = new List<object>();
            var list = ((GraphQLValue<IEnumerable<GraphQLValue>>)value).Value;

            foreach (var item in list)
                output.Add(GetValue(item));

            return output;
        }

        private string GetFieldEntryKey(GraphQLFieldSelection selection)
        {
            return selection.Alias != null ? selection.Alias.Value : selection.Name.Value;
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.Operation.Operation)
            {
                case OperationType.Query: return this.GraphQLSchema.RootType;
                default: throw new Exception("Can only execute queries");
            }
        }

        private void ResolveDefinition(ASTNode definition)
        {
            switch (definition.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    this.ResolveOperationDefinition(definition as GraphQLOperationDefinition); break;
                case ASTNodeKind.FragmentDefinition:
                    this.ResolveFragmentDefinition(definition as GraphQLFragmentDefinition); break;
                default: throw new Exception($"GraphQL cannot execute a request containing a {definition.Kind}.");
            }
        }

        private void ResolveFragmentDefinition(GraphQLFragmentDefinition graphQLFragmentDefinition)
        {
            this.Fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (this.Operation != null)
                throw new Exception("Must provide operation name if query contains multiple operations.");

            if (this.Operation == null)
                this.Operation = graphQLOperationDefinition;
        }
    }
}
