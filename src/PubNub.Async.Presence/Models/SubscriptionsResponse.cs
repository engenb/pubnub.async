namespace PubNub.Async.Presence.Models
{
	public class SubscriptionsResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }

		public string[] Channels { get; set; }
	}
}
