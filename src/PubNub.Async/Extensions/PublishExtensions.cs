using System.Threading.Tasks;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Extensions
{
	public static class PublishExtensions
	{
		public static async Task<PublishResponse> Publish<TContent>(
			this string channel,
			TContent message,
			bool recordHistory = true)
		{
			return await new PubNubClient(channel)
				.Publish(message, recordHistory)
				.ConfigureAwait(false);
		}

		public static async Task<PublishResponse> Publish<TContent>(
			this Channel channel,
			TContent message,
			bool recordHistory = true)
		{
			return await new PubNubClient(channel)
				.Publish(message, recordHistory)
				.ConfigureAwait(false);
		}

		public static async Task<PublishResponse> Publish<TContent>(
			this IPubNubClient client,
			TContent message,
			bool recordHistory = true)
		{
			return await PubNub.Environment
				.Resolve<IPublishService>(client)
				.Publish(message, recordHistory)
				.ConfigureAwait(false);
		}
	}
}
