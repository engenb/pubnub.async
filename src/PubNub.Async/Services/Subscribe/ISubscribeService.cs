using System.Threading.Tasks;
using PubNub.Async.Models.Subscribe;

namespace PubNub.Async.Services.Subscribe
{
    public interface ISubscribeService
    {
        Task<SubscribeResponse> Subscribe<TMessage>(MessageReceivedHandler<TMessage> handler);
        Task Unsubscribe<TMessage>(MessageReceivedHandler<TMessage> handler);
        Task Unsubscribe();
    }
}