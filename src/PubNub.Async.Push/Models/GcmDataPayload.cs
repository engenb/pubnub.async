using Newtonsoft.Json;

namespace PubNub.Async.Push.Models
{
    public class GcmDataPayload
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}