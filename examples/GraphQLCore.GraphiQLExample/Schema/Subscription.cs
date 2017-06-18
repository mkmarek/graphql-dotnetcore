namespace GraphQLCore.GraphiQLExample.Schema
{
    using Events;
    using Execution;
    using Models;
    using Services;
    using System.Linq;
    using Type;
    using Type.Complex;

    public class Subscription : GraphQLSubscriptionType
    {
        public Subscription() : base("Subscription", "", new InMemoryEventBus())
        {
            var service = new CharacterService();
            this.Field("characters", (Episode episode) => service.List(episode))
                .WithSubscriptionFilter((IContext<ICharacter> ctx, Episode episode) =>
                    ctx.Instance.AppearsIn != null && ctx.Instance.AppearsIn.Contains(episode) == true)
                .OnChannel("characters");

            this.Field("newDroid", ((IContext<Droid> ctx) => service.GetDroidById(ctx.Instance.Id)))
                .OnChannel("droid");
        }
    }
}