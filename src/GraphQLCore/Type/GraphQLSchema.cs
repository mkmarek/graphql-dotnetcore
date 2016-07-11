namespace GraphQLCore.Type
{
    using Execution;
    using Introspection;
    using Language;
    using Language.AST;
    using System.Linq;
    using Translation;

    public class GraphQLSchema : IGraphQLSchema
    {
        private IIntrospector introspector;
        private ISchemaObserver schemaObserver;

        public GraphQLSchema()
        {
            this.schemaObserver = new SchemaObserver();
            this.TypeTranslator = new TypeTranslator(this.schemaObserver);
            this.introspector = new Introspector(this.TypeTranslator);
            this.IntrospectedSchema = new IntrospectedSchemaType(this.schemaObserver, this.introspector, this);

            this.RegisterIntrospectionTypes();
        }

        public IntrospectedSchemaType IntrospectedSchema { get; private set; }
        public GraphQLObjectType MutationType { get; private set; }
        public GraphQLObjectType QueryType { get; private set; }
        public ITypeTranslator TypeTranslator { get; private set; }

        public void AddKnownType(GraphQLNullableType type)
        {
            this.schemaObserver.AddKnownType(type);
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

        public void Mutation(GraphQLObjectType root)
        {
            this.MutationType = root;
        }

        public void Query(GraphQLObjectType root)
        {
            this.QueryType = root;
        }

        internal IntrospectedType GetGraphQLType(string name)
        {
            return this.IntrospectedSchema.Introspect().Where(e => e.Name == name).FirstOrDefault();
        }

        private GraphQLDocument GetAst(string expression)
        {
            return new Parser(new Lexer()).Parse(new Source(expression));
        }

        private void RegisterIntrospectionTypes()
        {
            this.schemaObserver.AddKnownType(new IntrospectedTypeKindType());
            this.schemaObserver.AddKnownType(new IntrospectedTypeType());
            this.schemaObserver.AddKnownType(new IntrospectedFieldType());
            this.schemaObserver.AddKnownType(new IntrospectedInputValueType());
            this.schemaObserver.AddKnownType(new GraphQLEnumValue(null, null));
            this.schemaObserver.AddKnownType(this.IntrospectedSchema);
        }
    }
}