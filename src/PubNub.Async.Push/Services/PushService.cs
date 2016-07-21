using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Push.Models;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Push.Services
{
	public class PushService : IPushService
	{
		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

        private IAccessManager Access { get; }
        private IPublishService Publish { get; }

		protected const int DeviceSuccessCount = 1;
        protected const string DeviceSuccessMessage = "Modified Channels";

		public PushService(IPubNubClient client, IAccessManager access, IPublishService publish)
		{
			Environment = client.Environment;
			Channel = client.Channel;

		    Access = access;
			Publish = publish;
		}

		public async Task<PushResponse> Register(DeviceType type, string token)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				throw new ArgumentException("Cannot be null or empty", nameof(token));
			}

		    if (Channel.Secured && Environment.GrantCapable())
		    {
		        var grantResponse = await Access.Establish(AccessType.Write);
		        if (!grantResponse.Success)
		        {
		            //TODO: error!
		        }
		    }

			var requestUrl = BuildDeviceUrl(type, token, "add");
		    var httpResponse = await requestUrl
		        .AllowHttpStatus("403")
		        .GetAsync()
		        .ProcessResponse();

			return await HandleResponse(httpResponse);
		}

		public async Task<PushResponse> Revoke(DeviceType type, string token)
		{
			if (string.IsNullOrWhiteSpace(token))
			{
				throw new ArgumentException("Cannot be null or empty", nameof(token));
            }

            if (Channel.Secured && Environment.GrantCapable())
            {
                var grantResponse = await Access.Establish(AccessType.Write);
                if (!grantResponse.Success)
                {
                    //TODO: error!
                }
            }

            var requestUrl = BuildDeviceUrl(type, token, "remove");
		    var httpResponse = await requestUrl
		        .AllowHttpStatus("403")
		        .GetAsync()
		        .ProcessResponse();

			return await HandleResponse(httpResponse);
		}

		public Task<PublishResponse> PublishPushNotification(string message, bool isDebug = false)
		{
			if (Channel.Encrypted)
			{
				throw new InvalidOperationException("Push notifications should not be sent using an encrypted channel");
			}

			var payload = new PushPayload(message)
			{
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

            var requestUrl = Environment.Host
                .AppendPathSegments("v1", "push")
                .AppendPathSegments("sub-key", Environment.SubscribeKey)
                .AppendPathSegments("devices", token)
                .SetQueryParam("type", pushService)
                .SetQueryParam(action, Channel.Name);

		    if (!string.IsNullOrWhiteSpace(Environment.AuthenticationKey))
		    {
		        requestUrl.SetQueryParam("auth", Environment.AuthenticationKey);
		    }

		    return requestUrl;
		}

		private async Task<PushResponse> HandleResponse(HttpResponseMessage httpResponse)
        {
            var response = new PushResponse
            {
                Message = "Unknown error occurred while attempting to modify device registration"
            };

            if (httpResponse.IsSuccessStatusCode)
		    {
		        var content = await Task.FromResult(httpResponse)
		            .ReceiveJson<JArray>();

		        if (content?.Count == 2)
		        {
		            if (content[0].Value<int>() == DeviceSuccessCount
		                && content[1].Value<string>() == DeviceSuccessMessage)
		            {
		                response.Success = true;
                    }
                    response.Message = content[1].Value<string>();
                }
		    }
            else if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                var content = await Task.FromResult(httpResponse)
                    .ReceiveJson<PubNubForbiddenResponse>();
                response.Message = content.Message;
            }

			return response;
		}
	}
}