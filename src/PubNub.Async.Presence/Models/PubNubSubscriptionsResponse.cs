using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class PubNubSubscriptionsResponse
	{
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("service")]
		public string Service { get; set; }

		[JsonProperty("payload")]
		public PubNubSubscriptionsResponsePayload Payload { get; set; }
	}
}
