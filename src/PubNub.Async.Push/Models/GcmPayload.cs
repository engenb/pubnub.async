using Newtonsoft.Json;

namespace PubNub.Async.Push.Models
{
    public class GcmPayload
    {
        [JsonProperty(PropertyName = "data")]
        public GcmDataPayload Data { get; set; }
    }
}