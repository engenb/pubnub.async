using Autofac;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Subscribe;

namespace PubNub.Async.Autofac
{
    public class ResolveSubscription : IResolveSubscription
    {
        private IComponentContext Context { get; }

        public ResolveSubscription(IComponentContext context)
        {
            Context = context;
        }

        public Subscription<TMessage> Resolve<TMessage>(IPubNubEnvironment environment, Channel channel)
        {
            return (Subscription<TMessage>)Context.Resolve(
                typeof(Subscription<TMessage>),
                new TypedParameter(typeof(IPubNubEnvironment), environment),
                new TypedParameter(typeof(Channel), channel));
        }
    }
}
