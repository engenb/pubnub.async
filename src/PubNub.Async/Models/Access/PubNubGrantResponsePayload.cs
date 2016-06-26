using System.Collections.Generic;
using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
	public class PubNubGrantResponsePayload
	{
		[JsonProperty("ttl")]
		public int MintuesToExpire { get; set; }

		[JsonProperty("auths")]
		public IDictionary<string, PubNubGrantResponseAuths> Auths { get; set; }

		[JsonProperty("subscribe_key")]
		public string SubscribeKey { get; set; }

		[JsonProperty("level")]
		public string Level { get; set; }

		[JsonProperty("channel")]
		public string Channel { get; set; }
	}
}