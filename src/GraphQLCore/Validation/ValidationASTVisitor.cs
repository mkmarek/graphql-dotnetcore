namespace GraphQLCore.Validation
{
    using Language;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;
    using Type.Translation;

    public class ValidationASTVisitor : GraphQLAstVisitor
    {
        private GraphQLBaseType argumentType;
        private Stack<GraphQLFieldInfo> fieldStack;
        private IGraphQLSchema schema;
        private Stack<GraphQLBaseType> typeStack;

        public ValidationASTVisitor(IGraphQLSchema schema)
        {
            this.typeStack = new Stack<GraphQLBaseType>();
            this.fieldStack = new Stack<GraphQLFieldInfo>();

            this.schema = schema;
            this.SchemaRepository = schema.SchemaRepository;
            this.LiteralValueValidator = new LiteralValueValidator(this.SchemaRepository);
        }

        protected LiteralValueValidator LiteralValueValidator { get; set; }
        protected ISchemaRepository SchemaRepository { get; private set; }

        public override GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            this.argumentType = this.SchemaRepository.GetSchemaTypeFor(this.GetLastField()
                .Arguments.Single(e => e.Key == argument.Name.Value).Value.SystemType);

            return base.BeginVisitArgument(argument);
        }

        public override GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            var field = this.GetField(this.GetLastType(), selection.Name.Value);

            if (field != null)
            {
                this.fieldStack.Push(field);
                this.typeStack.Push(this.SchemaRepository.GetSchemaTypeFor(field.SystemType));

                return base.BeginVisitFieldSelection(selection);
            }

            return selection;
        }

        public override GraphQLInlineFragment BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            this.typeStack.Push(this.SchemaRepository.GetSchemaOutputTypeByName(inlineFragment.TypeCondition.Name.Value));

            return base.BeginVisitInlineFragment(inlineFragment);
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
            return base.BeginVisitSelectionSet(selectionSet);
        }

        public override GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection)
        {
            this.fieldStack.Pop();
            this.typeStack.Pop();

            return base.EndVisitFieldSelection(selection);
        }

        public GraphQLBaseType GetArgumentDefinition()
        {
            return this.argumentType;
        }

        public GraphQLFieldInfo GetLastField()
        {
            if (this.fieldStack.Count > 0)
                return this.fieldStack.Peek();

            return null;
        }

        public GraphQLBaseType GetLastType()
        {
            if (this.typeStack.Count > 0)
            {
                var type = this.typeStack.Peek();

                if (type is GraphQLList)
                {
                    return ((GraphQLList)type).MemberType;
                }

                return type;
            }

            return null;
        }

        private GraphQLFieldInfo GetField(GraphQLBaseType type, string name)
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