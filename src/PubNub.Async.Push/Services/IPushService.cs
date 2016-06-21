using PubNub.Async.Models.Publish;
using PubNub.Async.Push.Models;
using System.Threading.Tasks;

namespace PubNub.Async.Push.Services
{
    public interface IPushService
    {
        Task<PushResponse> Register(DeviceType type, string token);

        Task<PushResponse> Revoke(DeviceType type, string token);

        Task<PublishResponse> PublishPushNotification(string message, bool isDebug = false);

		Task<PublishResponse> PublishPushNotification(object message, bool isDebug = false);
	}
}