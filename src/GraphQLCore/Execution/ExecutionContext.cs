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
    using Utils;

    public class ExecutionContext : IDisposable
    {
        private GraphQLDocument ast;
        private FieldCollector fieldCollector;
        private Dictionary<string, GraphQLFragmentDefinition> fragments;
        private GraphQLOperationDefinition operation;
        private VariableResolver operationVariableResolver;
        private dynamic variables;

        internal GraphQLSchema GraphQLSchema { get; private set; }

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast)
        {
            this.GraphQLSchema = graphQLSchema;
            this.ast = ast;
            this.fragments = new Dictionary<string, GraphQLFragmentDefinition>();
            this.fieldCollector = new FieldCollector(this.fragments, this);
            this.variables = new ExpandoObject();
        }

        public ExecutionContext(GraphQLSchema graphQLSchema, GraphQLDocument ast, dynamic variables)
            : this(graphQLSchema, ast)
        {
            this.variables = variables;
        }

        public void Dispose()
        {
        }

        public dynamic Execute()
        {
            foreach (var definition in this.ast.Definitions)
                this.ResolveDefinition(definition);

            if (this.operation == null)
                throw new Exception("Must provide an operation.");

            this.operationVariableResolver = new VariableResolver(
                this.variables, this.GraphQLSchema.TypeTranslator, this.operation.VariableDefinitions);

            var type = this.GetOperationRootType();

            return this.ComposeResultForType(type, this.operation.SelectionSet);
        }

        public object[] FetchArgumentValues(LambdaExpression expression, IList<GraphQLArgument> arguments)
        {
            return ReflectionUtilities.GetParameters(expression)
                .Select(e => ReflectionUtilities.ChangeValueType(this.GetArgumentValue(arguments, e.Name), e.Type))
                .ToArray();
        }

        public object GetArgumentValue(IEnumerable<GraphQLArgument> arguments, string argumentName)
        {
            var argument = arguments.SingleOrDefault(e => e.Name.Value == argumentName);

            if (argument == null)
                return null;

            return this.GetValue(argument.Value);
        }

        public IEnumerable GetListValue(Language.AST.GraphQLValue value)
        {
            IList output = new List<object>();
            var list = ((GraphQLValue<IEnumerable<GraphQLValue>>)value).Value;

            foreach (var item in list)
                output.Add(this.GetValue(item));

            return output;
        }

        public object GetValue(GraphQLValue value)
        {
            var literalValue = this.GraphQLSchema.TypeTranslator.GetLiteralValue(value);

            if (literalValue != null)
                return literalValue;

            switch (value.Kind)
            {
                case ASTNodeKind.ListValue: return this.GetListValue(value);
                case ASTNodeKind.Variable: return this.operationVariableResolver.GetValue((GraphQLVariable)value);
                default: throw new NotImplementedException($"Unknoen kind {value.Kind}");
            }
        }

        public object InvokeWithArguments(IList<GraphQLArgument> arguments, LambdaExpression expression)
        {
            var argumentValues = this.FetchArgumentValues(expression, arguments);

            return expression.Compile().DynamicInvoke(argumentValues);
        }

        internal dynamic ComposeResultForType(GraphQLObjectType type, GraphQLSelectionSet selectionSet, object parentObject = null)
        {
            var scope = this.CreateScope(type, parentObject);
            return this.GetResultFromScope(type, selectionSet, scope);
        }

        internal FieldScope CreateScope(GraphQLObjectType type, object parentObject)
        {
            return new FieldScope(this, type, parentObject, this.operationVariableResolver);
        }

        internal dynamic GetResultFromScope(GraphQLObjectType type, GraphQLSelectionSet selectionSet, FieldScope scope)
        {
            var fields = this.fieldCollector.CollectFields(type, selectionSet);
            return scope.GetObject(fields);
        }

        private GraphQLObjectType GetOperationRootType()
        {
            switch (this.operation.Operation)
            {
                case OperationType.Query: return this.GraphQLSchema.QueryType;
                case OperationType.Mutation: return this.GraphQLSchema.MutationType;
                default: throw new Exception($"Can't execute type {this.operation.Operation}");
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
            this.fragments.Add(graphQLFragmentDefinition.Name.Value, graphQLFragmentDefinition);
        }

        private void ResolveOperationDefinition(GraphQLOperationDefinition graphQLOperationDefinition)
        {
            if (this.operation != null)
                throw new Exception("Must provide operation name if query contains multiple operations.");

            if (this.operation == null)
                this.operation = graphQLOperationDefinition;
        }
    }
}