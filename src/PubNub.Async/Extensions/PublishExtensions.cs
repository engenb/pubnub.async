using System.Threading.Tasks;
using PubNub.Async.Models;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Extensions
{
	public static class PublishExtensions
	{
		public static Task<PublishResponse> Publish<TMessage>(
			this string channel,
			TMessage message,
			bool recordHistory = true)
		{
			return new PubNubClient(channel)
				.Publish(message, recordHistory);
		}

		public static Task<PublishResponse> Publish<TMessage>(
			this Channel channel,
			TMessage message,
			bool recordHistory = true)
		{
			return new PubNubClient(channel)
				.Publish(message, recordHistory);
		}

		public static async Task<PublishResponse> Publish<TMessage>(
			this IPubNubClient client,
			TMessage message,
			bool recordHistory = true)
		{
			return await PubNub.Environment
				.Resolve<IPublishService>(client)
				.Publish(message, recordHistory)
				.ConfigureAwait(false);
		}
	}
}
