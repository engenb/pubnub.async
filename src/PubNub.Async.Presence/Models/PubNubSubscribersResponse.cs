using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class PubNubSubscribersResponse<TState> where TState : class
	{
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }
		
		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("service")]
		public string Service { get; set; }

		[JsonProperty("occupancy")]
		public long Occupancy { get; set; }

		[JsonProperty("uuids")]
		public Subscriber<TState>[] Subscribers { get; set; }
	}
}