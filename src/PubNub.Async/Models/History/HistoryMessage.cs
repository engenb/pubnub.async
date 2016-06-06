using Newtonsoft.Json;

namespace PubNub.Async.Models.History
{
	public class HistoryMessage<TContent>
	{
		[JsonProperty("timetoken")]
		public long? Sent { get; set; }

		[JsonProperty("message")]
		public TContent Content { get; set; }
	}
}