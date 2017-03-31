namespace GraphQLCore.Events
{
    using GraphQLCore.Language.AST;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System;

    public class EventBusSubscription
    {
        public LambdaExpression Filter { get; private set; }
        public string Channel { get; private set; }
        public string ClientId { get; private set; }
        public string OperationToExecute { get; private set; }
        public GraphQLDocument Document { get; private set; }
        public object Variables { get; private set; }

        private EventBusSubscription() { }

        public static EventBusSubscription Create<EntityType>(
            string channel,
            string clientId,
            string operationToExecute,
            object variables,
            Expression<Func<EntityType, bool>> filter,
            GraphQLDocument document)
        {
            var subscription = new EventBusSubscription();

            subscription.OperationToExecute = operationToExecute;
            subscription.Channel = channel;
            subscription.ClientId = clientId;
            subscription.Filter = filter;
            subscription.Document = document;
            subscription.Variables = variables;

            return subscription;
        }

        public static EventBusSubscription Create(
            string channel,
            string clientId,
            string operationToExecute,
            object variables,
            LambdaExpression filter,
            GraphQLDocument document)
        {
            var subscription = new EventBusSubscription();

            subscription.OperationToExecute = operationToExecute;
            subscription.Channel = channel;
            subscription.ClientId = clientId;
            subscription.Filter = filter;
            subscription.Document = document;
            subscription.Variables = variables;

            return subscription;
        }
    }
}
