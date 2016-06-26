namespace PubNub.Async.Models.History
{
	public class HistoryResponse<TMessage>
	{
		public string Error { get; set; }

		public HistoryMessage<TMessage>[] Messages { get; set; }
		public long Oldest { get; set; }
		public long Newest { get; set; }
	}
}