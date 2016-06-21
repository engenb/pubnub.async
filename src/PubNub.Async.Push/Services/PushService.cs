using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Push.Models;
using PubNub.Async.Services.Publish;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PubNub.Async.Push.Services
{
    public class PushService : IPushService
    {
        private IPubNubEnvironment Environment { get; }
        private Channel Channel { get; }
        private IPublishService Publish { get; }

        public const string DEVICE_SUCCESS_COUNT = "1";
        public const string DEVICE_SUCCESS_MESSAGE = "Modified Channels";

        public PushService(IPubNubClient client, IPublishService publish)
        {
			Environment = client.Environment;
			Channel = client.Channel;
			Publish = publish;
        }

        public async Task<PushResponse> Register(DeviceType type, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(token));
            }

            var requestUrl = BuildDeviceUrl(type, token, "add");
            var result = await requestUrl
                .ConfigureClient(s => s.AllowedHttpStatusRange = "*")
                .GetAsync()
                .ProcessResponse()
                .ReceiveString();

            return ParseDeviceResult(result);
        }

        public async Task<PushResponse> Revoke(DeviceType type, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(token));
            }

            var requestUrl = BuildDeviceUrl(type, token, "remove");
            var result = await requestUrl
                .ConfigureClient(s => s.AllowedHttpStatusRange = "403")
                .GetAsync()
                .ReceiveString();

            return ParseDeviceResult(result);
        }

        public Task<PublishResponse> PublishPushNotification(string message, bool isDebug = false)
		{
			if (Channel.Encrypted)
			{
				throw new InvalidOperationException("Push notifications should not be sent using an encrypted channel");
			}

			var payload = new PushPayload
            {
                Apns = new ApnsPayload
                {
                    Aps = new ApsPayload
                    {
                        Alert = message
                    }
                },
                Gcm = new GcmPayload
                {
                    Data = new GcmDataPayload
                    {
                        Message = message
                    }
                },
                IsDebug = isDebug
            };

			return Publish.Publish(payload, false);
		}

	    public Task<PublishResponse> PublishPushNotification(object message, bool isDebug = false)
	    {
		    return PublishPushNotification(JsonConvert.SerializeObject(message), isDebug);
	    }

	    private Url BuildDeviceUrl(DeviceType type, string token, string action)
        {
            var pushService = string.Empty;
            switch (type)
            {
                case DeviceType.Android:
                    pushService = "gcm";
                    break;

                case DeviceType.iOS:
                    pushService = "apns";
                    break;

                case DeviceType.Windows:
                    pushService = "mpns";
                    break;
            }

            return Environment.Host
                .AppendPathSegments("v1", "push")
                .AppendPathSegments("sub-key", Environment.SubscribeKey)
                .AppendPathSegments("devices", token)
                .SetQueryParam("type", pushService)
                .SetQueryParam(action, Channel.Name);
        }

        private PushResponse ParseDeviceResult(string result)
        {
            var response = new PushResponse
            {
                Message = "Unknown error occurred while attempting to register device"
            };

            if (result != null)
            {
                var parsedResult = JToken.Parse(result);
                if (parsedResult.Type == JTokenType.Array)
                {
                    var arrayValues = parsedResult.Children().Values<object>().ToList();
                    if (arrayValues.Count == 2
                        && arrayValues[0].ToString() == DEVICE_SUCCESS_COUNT
                        && arrayValues[1].ToString() == DEVICE_SUCCESS_MESSAGE)
                    {
                        response.Success = true;
                        response.Message = null;
                    }
                }
                else if (parsedResult.Type == JTokenType.Object)
                {
                    JToken errorToken;
                    if (((JObject)parsedResult).TryGetValue("error", out errorToken))
                    {
                        response.Message = errorToken.Value<string>();
                    }
                }
            }

            return response;
        }
    }
}