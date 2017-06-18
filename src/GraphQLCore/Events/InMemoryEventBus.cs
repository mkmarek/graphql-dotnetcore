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
        private Dictionary<string, EventBusSubscription> subscriptions;
        public event MessageReceived OnMessageReceived;

        public InMemoryEventBus()
        {
            this.subscriptions = new Dictionary<string, EventBusSubscription>();
        }

        public async Task Publish(object data, string channel)
        {
            if (this.OnMessageReceived == null)
                return;

            foreach (var subscription in this.subscriptions.Where(e => e.Value.Channel == channel).ToList())
            {
                if (subscription.Value.Filter == null || (bool)subscription.Value.Filter.Compile().DynamicInvoke(data))
                {
                    await this.OnMessageReceived(new OnMessageReceivedEventArgs()
                    {
                        Data = data,
                        ClientId = subscription.Value.ClientId,
                        SubscriptionId = subscription.Value.SubscriptionId,
                        Channel = subscription.Value.Channel,
                        Document = subscription.Value.Document
                    });
                }
            }
        }

        public async Task Subscribe(EventBusSubscription subscription)
        {
            await Task.Yield();

            var key = this.BuildKey(subscription.ClientId, subscription.SubscriptionId);

            if (!this.subscriptions.ContainsKey(key))
            {
                this.subscriptions.Add(key, subscription);
            }
        }

        public void Unsubscribe(string clientId, int subscriptionId)
        {
            var key = this.BuildKey(clientId, subscriptionId);

            if (this.subscriptions.ContainsKey(key))
            {
                this.subscriptions.Remove(key);
            }
        }

        public void Unsubscribe(string clientId)
        {
            var selectedSubscriptions = this.subscriptions.Where(e => e.Value.ClientId == clientId).ToList();

            foreach (var subscription in selectedSubscriptions)
            {
                this.subscriptions.Remove(subscription.Key);
            }
        }

        private string BuildKey(string clientId, int subscriptionId)
        {
            return clientId + "_" + subscriptionId;
        }
    }
}
