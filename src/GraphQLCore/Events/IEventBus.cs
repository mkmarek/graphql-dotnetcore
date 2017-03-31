using GraphQLCore.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GraphQLCore.Events
{
    public delegate void MessageReceived(OnMessageReceivedEventArgs args);

    public interface IEventBus
    {
        event MessageReceived OnMessageReceived;
        void Publish(object data, string channel);
        void Subscribe(EventBusSubscription eventBusSubscription);
    }
}
