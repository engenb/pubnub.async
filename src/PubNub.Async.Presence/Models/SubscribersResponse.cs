namespace PubNub.Async.Presence.Models
{
	public class SubscribersResponse<TState> where TState : class
	{
		public bool Success { get; set; }

		public string Message { get; set; }

		public long Occupancy { get; set; }

		public Subscriber<TState>[] Subscribers { get; set; }
	}
}
