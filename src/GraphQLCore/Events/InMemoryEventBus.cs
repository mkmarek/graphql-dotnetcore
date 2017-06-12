namespace GraphQLCore.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    public class InMemoryEventBus : IEventBus
    {
        private List<EventBusSubscription> subscriptions;
        public event MessageReceived OnMessageReceived;

        public InMemoryEventBus()
        {
            this.subscriptions = new List<EventBusSubscription>();
        }

        public async Task Publish(object data, string channel)
        {
            if (this.OnMessageReceived == null)
                return;

            foreach (var subscription in this.subscriptions.Where(e => e.Channel == channel).ToList())
            {
                if ((bool)subscription.Filter.Compile().DynamicInvoke(data))
                {
                    await this.OnMessageReceived(new OnMessageReceivedEventArgs()
                    {
                        ClientId = subscription.ClientId,
                        Channel = subscription.Channel,
                        Document = subscription.Document
                    });
                }
            }
        }

        public async Task Subscribe(EventBusSubscription subscription)
        {
            await Task.Yield();

            if (!this.subscriptions.Any(e =>
                e.ClientId == subscription.ClientId &&
                e.Filter.ToString() == e.Filter.ToString() &&
                e.Channel == e.Channel)) //TODO create operation comparer
                this.subscriptions.Add(subscription);
        }
    }
}
