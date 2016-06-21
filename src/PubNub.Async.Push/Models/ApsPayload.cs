using Newtonsoft.Json;

namespace PubNub.Async.Push.Models
{
    public class ApsPayload
    {
        [JsonProperty(PropertyName = "alert")]
        public string Alert { get; set; }
    }
}