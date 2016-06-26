namespace PubNub.Async.Presence.Models
{
	public class SessionStateResponse<TState> where TState : class
	{
		public bool Success { get; set; }
		public TState State { get; set; }
	}
}