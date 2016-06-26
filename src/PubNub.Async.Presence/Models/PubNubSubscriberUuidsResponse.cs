using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class PubNubSubscriberUuidsResponse
	{
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("occupancy")]
		public long Occupancy { get; set; }

		[JsonProperty("uuids")]
		public string[] Subscribers { get; set; }
	}
}
