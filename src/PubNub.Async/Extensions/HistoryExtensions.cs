using System.Threading.Tasks;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.History;
using PubNub.Async.Services.History;

namespace PubNub.Async.Extensions
{
	public static class HistoryExtensions
	{
		public static Task<HistoryResponse<TContent>> History<TContent>(
			this string channel,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return new PubNubClient(channel)
				.History<TContent>(start, end, count, order, includeTime);
		}

		public static Task<HistoryResponse<TContent>> History<TContent>(
			this Channel channel,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return new PubNubClient(channel)
				.History<TContent>(start, end, count, order, includeTime);
		}

		public static async Task<HistoryResponse<TContent>> History<TContent>(
			this IPubNubClient client,
			long? start = null,
			long? end = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			return await PubNub.Environment
				.Resolve<IHistoryService>(client)
				.History<TContent>(start, end, count, order, includeTime)
				.ConfigureAwait(false);
		}
	}
}