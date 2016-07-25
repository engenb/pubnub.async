using System.Threading.Tasks;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;

namespace PubNub.Async.Services.Subscribe
{
    public interface ISubscriptionRegistry
    {
        Subscription[] Get(string subscribeKey);

        void Register<TMessage>(IPubNubEnvironment environment, Channel channel, MessageReceivedHandler<TMessage> handler);
        void Unregister<TMessage>(IPubNubEnvironment environment, Channel channel, MessageReceivedHandler<TMessage> handler);
        void Unregister(IPubNubEnvironment environment, Channel channel);

        void MessageReceived(PubNubSubscribeResponseMessage message);
    }
}