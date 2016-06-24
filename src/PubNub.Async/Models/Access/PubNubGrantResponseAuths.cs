using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
	public class PubNubGrantResponseAuths
	{
		[JsonProperty("r")]
		public bool Read { get; set; }
		[JsonProperty("w")]
		public bool Write { get; set; }
	}
}