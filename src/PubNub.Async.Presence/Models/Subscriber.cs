using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class Subscriber<TState> where TState : class
	{
		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("state")]
		public TState State { get; set; }
	}
}
