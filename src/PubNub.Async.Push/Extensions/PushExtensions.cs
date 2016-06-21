using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Push.Models;
using PubNub.Async.Push.Services;
using System.Threading.Tasks;

namespace PubNub.Async.Push.Extensions
{
    public static class PushExtensions
    {
        public static async Task<PushResponse> RegisterDeviceForPush(
            this string channel,
            DeviceType type,
            string token)
        {
            return await new PubNubClient(channel)
                .RegisterDeviceForPush(type, token);
        }

        public static async Task<PushResponse> RegisterDeviceForPush(
            this Channel channel,
            DeviceType type,
            string token)
        {
            return await new PubNubClient(channel)
                .RegisterDeviceForPush(type, token);
        }

        public static async Task<PushResponse> RegisterDeviceForPush(
            this IPubNubClient client,
            DeviceType type,
            string token)
        {
            return await PubNub.Environment
                .Resolve<IPushService>(client)
                .Register(type, token);
        }

        public static async Task<PushResponse> RevokeDeviceForPush(
            this string channel,
            DeviceType type,
            string token)
        {
            return await new PubNubClient(channel)
                .RevokeDeviceForPush(type, token);
        }

        public static async Task<PushResponse> RevokeDeviceForPush(
            this Channel channel,
            DeviceType type,
            string token)
        {
            return await new PubNubClient(channel)
                .RevokeDeviceForPush(type, token);
        }

        public static async Task<PushResponse> RevokeDeviceForPush(
            this IPubNubClient client,
            DeviceType type,
            string token)
        {
            return await PubNub.Environment
                .Resolve<IPushService>(client)
                .Revoke(type, token);
        }

        public static async Task<PublishResponse> PublishPushNotification(
            this string channel,
            string message,
			bool isDebug = false)
        {
            return await new PubNubClient(channel)
                .PublishPushNotification(message, isDebug);
        }

        public static async Task<PublishResponse> PublishPushNotification(
            this Channel channel,
            string message,
			bool isDebug = false)
        {
            return await new PubNubClient(channel)
                .PublishPushNotification(message, isDebug);
        }

        public static async Task<PublishResponse> PublishPushNotification(
            this IPubNubClient client,
            string message,
			bool isDebug = false)
        {
            return await PubNub.Environment
                .Resolve<IPushService>(client)
                .PublishPushNotification(message, isDebug);
        }

        public static async Task<PublishResponse> PublishPushNotification(
            this string channel,
            object payload,
			bool isDebug = false)
        {
            return await new PubNubClient(channel)
                .PublishPushNotification(payload, isDebug);
        }

        public static async Task<PublishResponse> PublishPushNotification(
            this Channel channel,
            object payload,
			bool isDebug = false)
        {
            return await new PubNubClient(channel)
                .PublishPushNotification(payload, isDebug);
		}

		public static async Task<PublishResponse> PublishPushNotification(
			this IPubNubClient client,
			object message,
			bool isDebug = false)
		{
			return await PubNub.Environment
				.Resolve<IPushService>(client)
				.PublishPushNotification(message, isDebug);
		}
	}
}