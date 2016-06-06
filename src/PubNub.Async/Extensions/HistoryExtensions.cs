using System.Threading.Tasks;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.History;

namespace PubNub.Async.Extensions
{
	public static class HistoryExtensions
	{
		public static async Task<HistoryResponse<TContent>> History<TContent>(
			this string channel,
			long? start = null,
			long? end = null,
			int? count = null,
			bool reverse = false,
			bool includeTime = true)
		{
			return await new PubNubClient(channel)
				.History<TContent>(start, end, count, reverse, includeTime);
		}

		public static async Task<HistoryResponse<TContent>> History<TContent>(
			this Channel channel,
			long? start = null,
			long? end = null,
			int? count = null,
			bool reverse = false,
			bool includeTime = true)
		{
			return await new PubNubClient(channel)
				.History<TContent>(start, end, count, reverse, includeTime);
		}

		public static async Task<HistoryResponse<TContent>> History<TContent>(
			this IPubNubClient client,
			long? start = null,
			long? end = null,
			int? count = null,
			bool reverse = false,
			bool includeTime = true)
		{
			return await PubNub.GlobalSettings
				.HistoryFactory(client)
				.History<TContent>(start, end, count, reverse, includeTime);
		}
	}
}