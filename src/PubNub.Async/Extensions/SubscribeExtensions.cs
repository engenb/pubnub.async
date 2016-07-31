using System.Threading.Tasks;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Subscribe;

namespace PubNub.Async.Extensions
{
	public static class SubscribeExtensions
	{
		public static Task<SubscribeResponse> Subscribe<TMessage>(
			this string channel,
			MessageReceivedHandler<TMessage> handler)
		{
			return new PubNubClient(channel)
				.Subscribe(handler);
		}

		public static Task<SubscribeResponse> Subscribe<TMessage>(
			this Channel channel,
			MessageReceivedHandler<TMessage> handler)
		{
			return new PubNubClient(channel)
				.Subscribe(handler);
		}

		public static async Task<SubscribeResponse> Subscribe<TMessage>(
			this IPubNubClient client,
			MessageReceivedHandler<TMessage> handler)
		{
			return await PubNub.Environment
				.Resolve<ISubscribeService>(client)
				.Subscribe(handler)
				.ConfigureAwait(false);
		}

		public static async Task Unsubscribe<TMessage>(
			this string channel,
			MessageReceivedHandler<TMessage> handler)
		{
			await new PubNubClient(channel)
				.Unsubscribe(handler);
		}

		public static async Task Unsubscribe<TMessage>(
			this Channel channel,
			MessageReceivedHandler<TMessage> handler)
		{
			await new PubNubClient(channel)
				.Unsubscribe(handler);
		}

		public static async Task Unsubscribe<TMessage>(
			this IPubNubClient client,
			MessageReceivedHandler<TMessage> handler)
		{
			await PubNub.Environment
				.Resolve<ISubscribeService>(client)
				.Unsubscribe(handler)
				.ConfigureAwait(false);
		}

		public static async Task Unsubscribe(
			this string channel)
		{
			await new PubNubClient(channel)
				.Unsubscribe();
		}

		public static async Task Unsubscribe(
			this Channel channel)
		{
			await new PubNubClient(channel)
				.Unsubscribe();
		}

		public static async Task Unsubscribe(
			this IPubNubClient client)
		{
			await PubNub.Environment
				.Resolve<ISubscribeService>(client)
				.Unsubscribe()
				.ConfigureAwait(false);
		}
	}
}
