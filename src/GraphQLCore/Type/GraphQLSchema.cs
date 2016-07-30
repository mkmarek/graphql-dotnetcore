namespace GraphQLCore.Type
{
    using Execution;
    using Introspection;
    using Language;
    using Language.AST;
    using System;
    using System.Linq;
    using Translation;

    public class GraphQLSchema : IGraphQLSchema
    {
        public ISchemaRepository SchemaRepository { get; private set; }

        public GraphQLSchema()
        {
            this.SchemaRepository = new SchemaRepository();
            this.IntrospectedSchema = new IntrospectedSchemaType(this.SchemaRepository, this);

            this.RegisterIntrospectionTypes();
        }

        public IntrospectedSchemaType IntrospectedSchema { get; private set; }
        public GraphQLObjectType MutationType { get; private set; }

        public GraphQLObjectType QueryType { get; private set; }

        public void AddKnownType(GraphQLBaseType type)
        {
            this.SchemaRepository.AddKnownType(type);
        }

        public dynamic Execute(string expression)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression)))
            {
                return context.Execute();
            }
        }

        public dynamic Execute(string expression, dynamic variables)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression), variables))
            {
                return context.Execute();
            }
        }

        public dynamic Execute(string expression, dynamic variables, string operationToExecute)
        {
            using (var context = new ExecutionContext(this, this.GetAst(expression), variables))
            {
                return context.Execute(operationToExecute);
            }
        }

        public void Mutation(GraphQLObjectType root)
        {
            this.MutationType = root;
        }

        public object Execute(object multipleOperationQuery)
        {
            throw new NotImplementedException();
        }

        public void Query(GraphQLObjectType root)
        {
            this.QueryType = root;
            this.SetupIntrospectionFields(root);
        }

        internal IntrospectedType IntrospectType(string name)
        {
            return this.IntrospectedSchema.IntrospectAllSchemaTypes().Where(e => e.Name == name).FirstOrDefault();
        }

        private GraphQLDocument GetAst(string expression)
        {
            return new Parser(new Lexer()).Parse(new Source(expression));
        }

        private void RegisterIntrospectionTypes()
        {
            this.SchemaRepository.AddKnownType(new IntrospectedTypeKindType());
            this.SchemaRepository.AddKnownType(new IntrospectedTypeType());
            this.SchemaRepository.AddKnownType(new IntrospectedFieldType());
            this.SchemaRepository.AddKnownType(new IntrospectedInputValueType());
            this.SchemaRepository.AddKnownType(new GraphQLEnumValue(null, null));
            this.SchemaRepository.AddKnownType(this.IntrospectedSchema);
        }

        private void SetupIntrospectionFields(GraphQLObjectType objectType)
        {
        }
    }
}