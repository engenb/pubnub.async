using System.Net;
using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
    public class PubNubForbiddenResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("payload")]
        public PubNubForbiddenResponsePayload Payload { get; set; }
        [JsonProperty("error")]
        public bool Error { get; set; }
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }
    }
}
