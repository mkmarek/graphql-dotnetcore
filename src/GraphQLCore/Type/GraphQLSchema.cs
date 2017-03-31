namespace GraphQLCore.Type
{
    using Execution;
    using GraphQLCore.Events;
    using GraphQLCore.Type.Complex;
    using GraphQLCore.Type.Directives;
    using Introspection;
    using Language;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Translation;

    public delegate void SubscriptionMessageReceived(dynamic data);

    public class GraphQLSchema : IGraphQLSchema
    {
        public ISchemaRepository SchemaRepository { get; private set; }
        public event SubscriptionMessageReceived OnSubscriptionMessageReceived;

        public GraphQLSchema()
        {
            this.SchemaRepository = new SchemaRepository();
            this.IntrospectedSchema = new IntrospectedSchemaType(this.SchemaRepository, this);

            this.RegisterIntrospectionTypes();
            this.RegisterDefaultDirectives();
        }

        public IntrospectedSchemaType IntrospectedSchema { get; private set; }
        public GraphQLObjectType MutationType { get; private set; }
        public GraphQLSubscriptionType SubscriptionType { get; private set; }
        public GraphQLObjectType QueryType { get; private set; }

        public void AddKnownType(GraphQLBaseType type)
        {
            this.SchemaRepository.AddKnownType(type);
        }

        public dynamic Execute(string expression)
        {
            using (var context = new ExecutionManager(this, this.GetAst(expression)))
            {
                return context.Execute();
            }
        }

        public dynamic Execute(string expression, dynamic variables)
        {
            using (var context = new ExecutionManager(this, this.GetAst(expression), variables))
            {
                return context.Execute();
            }
        }

        public dynamic Execute(string expression, dynamic variables, string operationToExecute)
        {
            using (var context = new ExecutionManager(this, this.GetAst(expression), variables))
            {
                return context.Execute(operationToExecute);
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

        public void Subscription(GraphQLSubscriptionType root)
        {
            this.SubscriptionType = root;
            this.SubscriptionType.EventBus.OnMessageReceived += InvokeSubscriptionMessageReceived;
        }

        public IntrospectedType IntrospectType(string name)
        {
            return this.IntrospectedSchema.IntrospectAllSchemaTypes().Where(e => e.Name == name).FirstOrDefault();
        }

        public void AddOrReplaceDirective(GraphQLDirectiveType directive)
        {
            this.SchemaRepository.AddOrReplaceDirective(directive);
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
            this.SchemaRepository.AddKnownType(new IntrospectedDirectiveType());
            this.SchemaRepository.AddKnownType(new IntrospectedDirectiveLocationType());
            this.SchemaRepository.AddKnownType(this.IntrospectedSchema);
        }

        private void RegisterDefaultDirectives()
        {
            this.SchemaRepository.AddOrReplaceDirective(new GraphQLIncludeDirectiveType());
            this.SchemaRepository.AddOrReplaceDirective(new GraphQLSkipDirectiveType());
        }

        private void InvokeSubscriptionMessageReceived(OnMessageReceivedEventArgs args)
        {
            using (var context = GetExecutionContext(args))
            {
                foreach (var definition in args.Document.Definitions)
                    context.ResolveDefinition(definition, args.OperationToExecute);

                var data = context.ComposeResultForQueryAndMutation(this.SubscriptionType, context.Operation);

                this.OnSubscriptionMessageReceived?.Invoke(data);
            }
        }

        private ExecutionManager GetExecutionContext(OnMessageReceivedEventArgs args)
        {
            if (args.Variables == null)
                return new ExecutionManager(this, args.Document);

            return new ExecutionManager(this, args.Document, args.Variables);
        }
    }
}