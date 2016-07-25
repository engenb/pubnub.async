using System.Threading.Tasks;
using PubNub.Async.Models;
using PubNub.Async.Models.History;
using PubNub.Async.Services.History;

namespace PubNub.Async.Extensions
{
	public static class HistoryExtensions
	{
		public static Task<HistoryResponse<TMessage>> History<TMessage>(
			this string channel,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return new PubNubClient(channel)
				.History<TMessage>(start, end, count, order, includeTime);
		}

		public static Task<HistoryResponse<TMessage>> History<TMessage>(
			this Channel channel,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return new PubNubClient(channel)
				.History<TMessage>(start, end, count, order, includeTime);
		}

		public static async Task<HistoryResponse<TMessage>> History<TMessage>(
			this IPubNubClient client,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return await PubNub.Environment
				.Resolve<IHistoryService>(client)
				.History<TMessage>(start, end, count, order, includeTime)
				.ConfigureAwait(false);
		}
	}
}