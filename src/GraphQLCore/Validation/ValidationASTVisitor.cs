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
        protected IGraphQLSchema schema;
        private GraphQLScalarType argumentType;
        private Dictionary<string, GraphQLScalarType> argumentTypes;
        private Stack<string> fieldStack;
        private Stack<GraphQLObjectType> parentTypeStack;
        private ITypeTranslator translator;
        private Stack<GraphQLObjectType> typeStack;

        public ValidationASTVisitor(IGraphQLSchema schema)
        {
            this.schema = schema;
            this.parentTypeStack = new Stack<GraphQLObjectType>();
            this.typeStack = new Stack<GraphQLObjectType>();
            this.fieldStack = new Stack<string>();
            this.argumentTypes = new Dictionary<string, GraphQLScalarType>();
            this.translator = schema.TypeTranslator;
        }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            var fieldName = this.fieldStack.Peek();
            this.argumentType = translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(fieldName).Arguments
                .SingleOrDefault(e => e.Key == argument.Name.Value).Value;
            this.argumentTypes.Remove(argument.Name.Value);

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Push(selection.Name.Value);

            argumentTypes.Clear();
            if (selection.Name?.Value != null)
            {
                var arguments = translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Arguments;
                if (arguments != null)
                {
                    foreach (var argument in translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Arguments)
                        argumentTypes.Add(argument.Key, argument.Value);
                }
            }

            var obj = translator.GetObjectTypeTranslatorFor(this.typeStack.Peek()).GetField(selection.Name.Value).Type as GraphQLObjectType;
            if (obj != null)
                this.typeStack.Push(obj);

            return base.BeginVisitFieldSelection(selection);
        }

        public override GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            switch (definition.Operation)
            {
                case OperationType.Query: this.typeStack.Push(this.schema.QueryType); break;
                default: throw new NotImplementedException();
            }

            definition = base.BeginVisitOperationDefinition(definition);
            this.typeStack.Pop();

            return definition;
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            fieldStack.Pop();
            argumentTypes.Clear();

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