using System.Threading.Tasks;
using PubNub.Async.Models.History;

namespace PubNub.Async.Services.History
{
	public interface IHistoryService
	{
		Task<HistoryResponse<TContent>> History<TContent>(
			long? first = null,
			long? last = null,
			int? count = null,
			bool reverse = false,
			bool includeTime = true);
	}
}