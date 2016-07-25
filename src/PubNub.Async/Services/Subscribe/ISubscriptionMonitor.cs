using System.Threading.Tasks;
using PubNub.Async.Configuration;

namespace PubNub.Async.Services.Subscribe
{
    public interface ISubscriptionMonitor
    {
        void Register(IPubNubEnvironment environment, long subscribeTimeToken);
        Task Start(IPubNubEnvironment environment);
        Task Stop(IPubNubEnvironment enviornment);
    }
}