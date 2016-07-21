using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
    public class PubNubForbiddenResponsePayload
    {
        [JsonProperty("channels")]
        public string[] Channels { get; set; }
    }
}
