namespace GraphQLCore.Events
{
    using System.Threading.Tasks;

    public delegate Task MessageReceived(OnMessageReceivedEventArgs args);

    public interface IEventBus
    {
        event MessageReceived OnMessageReceived;
        Task Publish(object data, string channel);
        Task Subscribe(EventBusSubscription eventBusSubscription);
        void Unsubscribe(string clientId, string subscriptionId);
        void Unsubscribe(string clientId);
    }
}
