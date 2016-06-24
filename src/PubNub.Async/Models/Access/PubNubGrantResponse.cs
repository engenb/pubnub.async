using System.Collections;
using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
	public class PubNubGrantResponse
	{
		[JsonProperty("status")]
		public HttpStatusCode Status { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("payload")]
		public PubNubGrantResponsePayload Paylaod { get; set; }
		[JsonProperty("service")]
		public string Service { get; set; }
	}
}
