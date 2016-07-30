namespace GraphQLCore.Validation
{
    using Language;
    using Language.AST;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Type;
    using Type.Complex;
    using Type.Introspection;
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
                case OperationType.Mutation: this.typeStack.Push(this.schema.MutationType); break;
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

        public override GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            var fragmentType = this.SchemaRepository.GetSchemaOutputTypeByName(node.TypeCondition.Name.Value);

            if (fragmentType != null)
            {
                this.typeStack.Push(fragmentType);
                return base.BeginVisitFragmentDefinition(node);
            }

            return node;
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

        private GraphQLFieldInfo GetField(GraphQLBaseType type, string fieldName)
        {
            if (this.IsQueryRootType(type))
            {
                if (fieldName == "__schema")
                    return this.GetIntrospectedSchemaField();

                if (fieldName == "__type")
                    return this.GetIntrospectedTypeField();
            }

            if (type is GraphQLCore.Type.GraphQLNonNullType)
                return this.GetField(((GraphQLCore.Type.GraphQLNonNullType)type).UnderlyingNullableType, fieldName);

            if (type is GraphQLInputObjectType)
                return ((GraphQLInputObjectType)type).GetFieldInfo(fieldName);

            if (type is GraphQLComplexType)
                return ((GraphQLComplexType)type).GetFieldInfo(fieldName);

            return null;
        }

        private GraphQLFieldInfo GetIntrospectedTypeField()
        {
            return GraphQLObjectTypeFieldInfo.CreateResolverFieldInfo(
                "__type",
                (Expression<Func<string, IntrospectedType>>)((string name) => this.schema.IntrospectType(name)));
        }

        private GraphQLFieldInfo GetIntrospectedSchemaField()
        {
            return GraphQLObjectTypeFieldInfo.CreateResolverFieldInfo(
                "__schema",
                (Expression<Func<IntrospectedSchemaType>>)(() => this.schema.IntrospectedSchema));
        }

        private bool IsQueryRootType(GraphQLBaseType type)
        {
            return type == this.schema.QueryType;
        }
    }
}