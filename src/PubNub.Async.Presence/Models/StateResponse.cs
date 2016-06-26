using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class StateResponse<TState>
	{
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty("uuid")]
		public string Uuid { get; set; }

		[JsonProperty("service")]
		public string Service { get; set; }

		[JsonProperty("channel")]
		public string Channel { get; set; }

		[JsonProperty("payload")]
		public TState Payload { get; set; }
	}
}