namespace PubNub.Async.Models.Publish
{
	public class PublishResponse
	{
        public bool Success { get; set; }
		public string Message { get; set; }
		public long Sent { get; set; }
	}
}
