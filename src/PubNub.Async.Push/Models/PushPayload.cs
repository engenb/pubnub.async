using Newtonsoft.Json;

namespace PubNub.Async.Push.Models
{
    public class PushPayload
    {
        [JsonProperty(PropertyName = "pn_apns")]
        public ApnsPayload Apns { get; set; }

        [JsonProperty(PropertyName = "pn_gcm")]
        public GcmPayload Gcm { get; set; }

        [JsonProperty(PropertyName = "pn_debug")]
        public bool IsDebug { get; set; }
    }
}