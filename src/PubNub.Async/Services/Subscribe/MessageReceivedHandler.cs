using System.Threading.Tasks;

namespace PubNub.Async.Services.Subscribe
{
    public delegate Task MessageReceivedHandler<TMessage>(MessageReceivedEventArgs<TMessage> args);
}
