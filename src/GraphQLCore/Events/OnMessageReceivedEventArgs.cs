using System;
using System.Collections.Generic;
using System.Text;
using GraphQLCore.Language.AST;

namespace GraphQLCore.Events
{
    public class OnMessageReceivedEventArgs
    {
        public string ClientId { get; set; }
        public int SubscriptionId { get; set; }
        public string Channel { get; set; }
        public GraphQLDocument Document { get; set; }
        public string OperationToExecute { get; set; }
        public dynamic Variables { get; internal set; }
        public object Data { get; internal set; }
    }
}
