using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PubNub.Async.Models.Access
{
    public class PubNubForbiddenResponsePayload
    {
        [JsonProperty("channels")]
        public string[] Channels { get; set; }
    }
}
