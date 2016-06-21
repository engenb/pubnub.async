using Newtonsoft.Json;

namespace PubNub.Async.Push.Models
{
    public class ApnsPayload
    {
        [JsonProperty(PropertyName = "aps")]
        public ApsPayload Aps { get; set; }
    }
}