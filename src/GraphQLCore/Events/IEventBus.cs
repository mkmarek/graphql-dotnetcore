using GraphQLCore.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphQLCore.Events
{
    public delegate Task MessageReceived(OnMessageReceivedEventArgs args);

    public interface IEventBus
    {
        event MessageReceived OnMessageReceived;
        Task Publish(object data, string channel);
        Task Subscribe(EventBusSubscription eventBusSubscription);
        void Unsubscribe(string clientId, int subscriptionId);
        void Unsubscribe(string clientId);
    }
}
