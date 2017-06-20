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
    using System.Threading.Tasks;
    using Translation;

    public delegate Task SubscriptionMessageReceived(string clientId, int subscriptionId, dynamic data);

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
            return this.ExecuteAsync(expression).GetAwaiter().GetResult();
        }

        public dynamic Execute(string expression, dynamic variables)
        {
            return this.ExecuteAsync(expression, variables).GetAwaiter().GetResult();
        }

        public dynamic Execute(string expression, dynamic variables, string operationToExecute)
        {
            return this.ExecuteAsync(expression, variables, operationToExecute).GetAwaiter().GetResult();
        }

        public dynamic Execute(
            string expression, dynamic variables, string operationToExecute, string clientId, int subscriptionId)
        {
            return this.ExecuteAsync(expression, variables, operationToExecute, clientId, subscriptionId)
                .GetAwaiter().GetResult();
        }

        public async Task<dynamic> ExecuteAsync(string expression)
        {
            using (var context = new ExecutionManager(this, expression))
            {
                return await context.ExecuteAsync();
            }
        }

        public async Task<dynamic> ExecuteAsync(string expression, dynamic variables)
        {
            using (var context = new ExecutionManager(this, expression, variables))
            {
                return await context.ExecuteAsync();
            }
        }

        public async Task<dynamic> ExecuteAsync(
            string expression, dynamic variables, string operationToExecute)
        {
            using (var context = new ExecutionManager(this, expression, variables))
            {
                return await context.ExecuteAsync(operationToExecute);
            }
        }

        public async Task<dynamic> ExecuteAsync(
            string expression, dynamic variables, string operationToExecute, string clientId, int subscriptionId)
        {
            using (var context = new ExecutionManager(
                this, expression, variables, clientId, subscriptionId))
            {
                return await context.ExecuteAsync(operationToExecute);
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
            this.SubscriptionType.EventBus.OnMessageReceived += this.InvokeSubscriptionMessageReceived;
        }

        public IntrospectedType IntrospectType(string name)
        {
            return this.IntrospectedSchema.IntrospectAllSchemaTypes().Where(e => e.Name == name).FirstOrDefault();
        }

        public void AddOrReplaceDirective(GraphQLDirectiveType directive)
        {
            this.SchemaRepository.AddOrReplaceDirective(directive);
        }

        public void Unsubscribe(string clientId, int subscriptionId)
        {
            this.SubscriptionType?.EventBus?.Unsubscribe(clientId, subscriptionId);
        }

        public void Unsubscribe(string clientId)
        {
            this.SubscriptionType?.EventBus?.Unsubscribe(clientId);
        }

        private void RegisterIntrospectionTypes()
        {
            this.SchemaRepository.AddKnownType(new IntrospectedTypeKindType());
            this.SchemaRepository.AddKnownType(new IntrospectedTypeType());
            this.SchemaRepository.AddKnownType(new IntrospectedFieldType());
            this.SchemaRepository.AddKnownType(new IntrospectedInputValueType());
            this.SchemaRepository.AddKnownType(new IntrospectedEnumValueType());
            this.SchemaRepository.AddKnownType(new IntrospectedDirectiveType());
            this.SchemaRepository.AddKnownType(new IntrospectedDirectiveLocationType());
            this.SchemaRepository.AddKnownType(this.IntrospectedSchema);
        }

        private void RegisterDefaultDirectives()
        {
            this.SchemaRepository.AddOrReplaceDirective(new GraphQLIncludeDirectiveType());
            this.SchemaRepository.AddOrReplaceDirective(new GraphQLSkipDirectiveType());
        }

        private async Task InvokeSubscriptionMessageReceived(OnMessageReceivedEventArgs args)
        {
            using (var context = this.GetExecutionContext(args))
            {
                foreach (var definition in args.Document.Definitions)
                    context.ResolveDefinition(definition, args.OperationToExecute);

                var data = await context.ComposeResultForQuery(this.SubscriptionType, context.Operation, args.Data);

                await this.OnSubscriptionMessageReceived?.Invoke(args.ClientId, args.SubscriptionId, data);
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