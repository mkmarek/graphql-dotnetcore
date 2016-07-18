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
        private GraphQLBaseType argumentType;
        private Stack<string> fieldStack;
        private GraphQLBaseType lastType;
        private IGraphQLSchema schema;
        private ISchemaRepository schemaRepository;
        private Stack<GraphQLBaseType> typeStack;

        public ValidationASTVisitor(IGraphQLSchema schema)
        {
            this.typeStack = new Stack<GraphQLBaseType>();
            this.fieldStack = new Stack<string>();

            this.schema = schema;
            this.schemaRepository = schema.SchemaRepository;
            this.LiteralValueValidator = new LiteralValueValidator(this.schemaRepository);
        }

        protected LiteralValueValidator LiteralValueValidator { get; set; }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            this.argumentType = this.schemaRepository.GetSchemaTypeFor(this.GetField(this.typeStack.Peek(), this.fieldStack.Peek())
                .Arguments.Single(e => e.Key == argument.Name.Value).Value.Type);

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Push(selection.Name.Value);

            var systemType = this.GetField(this.typeStack.Peek(), this.fieldStack.Peek()).SystemType;
            this.lastType = this.schemaRepository.GetSchemaTypeFor(systemType);

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

            if (this.typeStack.Count > 0)
                this.typeStack.Pop();

            return definition;
        }

        public override GraphQLSelectionSet BeginVisitSelectionSet(GraphQLSelectionSet selectionSet)
        {
            if (this.lastType is GraphQLComplexType || this.lastType is GraphQLInputObjectType)
                this.typeStack.Push(this.lastType);

            return base.BeginVisitSelectionSet(selectionSet);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Pop();

            if (this.typeStack.Count > 0)
                this.typeStack.Pop();

            return base.EndVisitFieldSelection(selection);
        }

        public GraphQLBaseType GetArgumentDefinition()
        {
            return this.argumentType;
        }

        private GraphQLObjectTypeFieldInfo GetField(GraphQLBaseType type, string name)
        {
            if (type is GraphQLCore.Type.GraphQLNonNullType)
                return this.GetField(((GraphQLCore.Type.GraphQLNonNullType)type).UnderlyingNullableType, name);

            if (type is GraphQLInputObjectType)
                return ((GraphQLInputObjectType)type).GetFieldInfo(name);

            if (type is GraphQLComplexType)
                return ((GraphQLComplexType)type).GetFieldInfo(name);

            return null;
        }
    }
}