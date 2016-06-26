namespace PubNub.Async.Presence.Models
{
	public class PresenceResponse<TState> where TState : class
	{
		public bool Success { get; set; }
		public TState State { get; set; }
	}
}