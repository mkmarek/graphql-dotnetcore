namespace GraphQLCore.Events
{
    using GraphQLCore.Language.AST;

    public class OnMessageReceivedEventArgs
    {
        public string ClientId { get; set; }
        public string SubscriptionId { get; set; }
        public string Channel { get; set; }
        public GraphQLDocument Document { get; set; }
        public string OperationToExecute { get; set; }
        public dynamic Variables { get; internal set; }
        public object Data { get; internal set; }
    }
}
