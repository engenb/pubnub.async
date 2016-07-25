using Newtonsoft.Json;

namespace PubNub.Async.Models.Subscribe
{
    public class PubNubSubscribeResponseTime
    {
        [JsonProperty("t")]
        public long TimeToken { get; set; }
        [JsonProperty("r")]
        public bool Replicate { get; set; }
    }
}
