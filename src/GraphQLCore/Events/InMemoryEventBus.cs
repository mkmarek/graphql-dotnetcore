namespace GraphQLCore.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    public class InMemoryEventBus : IEventBus
    {
        private List<EventBusSubscription> subscriptions;
        public event MessageReceived OnMessageReceived;

        public InMemoryEventBus()
        {
            this.subscriptions = new List<EventBusSubscription>();
        }

        public void Publish(object data, string channel)
        {
            if (this.OnMessageReceived == null)
                return;

            foreach (var subscription in this.subscriptions.ToList())
            {
                if ((bool)subscription.Filter.Compile().DynamicInvoke(data))
                {
                    this.OnMessageReceived(new OnMessageReceivedEventArgs()
                    {
                        ClientId = subscription.ClientId,
                        Channel = subscription.Channel,
                        Document = subscription.Document
                    });
                }
            }
        }

        public void Subscribe(EventBusSubscription subscription)
        {
            if (!this.subscriptions.Any(e =>
                e.ClientId == subscription.ClientId &&
                e.Filter.ToString() == e.Filter.ToString() &&
                e.Channel == e.Channel)) //TODO create operation comparer
                this.subscriptions.Add(subscription);
        }
    }
}
