using Newtonsoft.Json;

namespace PubNub.Async.Models.Subscribe
{
    public class PubNubSubscribeResponse
    {
        [JsonProperty("t")]
        public PubNubSubscribeResponseTime SubscribeTime { get; set; }
        [JsonProperty("m")]
        public PubNubSubscribeResponseMessage[] Messages { get; set; }
    }
}
