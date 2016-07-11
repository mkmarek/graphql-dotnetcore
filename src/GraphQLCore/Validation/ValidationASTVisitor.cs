namespace GraphQLCore.Validation
{
    using Language;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Translation;

    public class ValidationASTVisitor : GraphQLAstVisitor
    {
        protected IGraphQLSchema Schema { get; set; }

        private GraphQLScalarType argumentType;
        private Dictionary<string, GraphQLScalarType> argumentTypes;
        private Stack<string> fieldStack;
        private Stack<GraphQLObjectType> parentTypeStack;
        private ITypeTranslator translator;
        private Stack<GraphQLObjectType> typeStack;

        public ValidationASTVisitor(IGraphQLSchema schema)
        {
            this.Schema = schema;
            this.parentTypeStack = new Stack<GraphQLObjectType>();
            this.typeStack = new Stack<GraphQLObjectType>();
            this.fieldStack = new Stack<string>();
            this.argumentTypes = new Dictionary<string, GraphQLScalarType>();
            this.translator = schema.TypeTranslator;
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var fieldName = this.fieldStack.Peek();
            this.argumentType = this.translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(fieldName).Arguments
                .SingleOrDefault(e => e.Key == argument.Name.Value).Value;
            this.argumentTypes.Remove(argument.Name.Value);

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Push(selection.Name.Value);

            this.argumentTypes.Clear();
            if (selection.Name?.Value != null)
            {
                var arguments = this.translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Arguments;
                if (arguments != null)
                {
                    foreach (var argument in this.translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Arguments)
                        this.argumentTypes.Add(argument.Key, argument.Value);
                }
            }

            var obj = this.translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Type as GraphQLObjectType;
            if (obj != null)
                this.typeStack.Push(obj);

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            switch (definition.Operation)
            {
                case OperationType.Query: this.typeStack.Push(this.Schema.QueryType); break;
                default: throw new NotImplementedException();
            }

            definition = base.BeginVisitOperationDefinition(definition);
            this.typeStack.Pop();

            return definition;
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Pop();
            this.argumentTypes.Clear();

            return base.EndVisitFieldSelection(selection);
        }

        public GraphQLScalarType GetArgumentDefinition()
        {
            return this.argumentType;
        }

        public IEnumerable<GraphQLScalarType> GetUnvisitedArguments()
        {
            return this.argumentTypes
                .Select(e => e.Value)
                .ToList();
        }
    }
}