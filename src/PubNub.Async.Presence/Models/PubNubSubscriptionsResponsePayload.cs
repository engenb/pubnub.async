using Newtonsoft.Json;

namespace PubNub.Async.Presence.Models
{
	public class PubNubSubscriptionsResponsePayload
	{
		[JsonProperty("channels")]
		public string[] Channels { get; set; }
	}
}
