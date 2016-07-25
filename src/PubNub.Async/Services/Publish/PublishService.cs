using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLCrypto;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;

namespace PubNub.Async.Services.Publish
{
	public class PublishService : IPublishService
	{
		private ICryptoService Crypto { get; }
		private IAccessManager Access { get; }

		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

		public PublishService(
			IPubNubClient client,
			ICryptoService crypto,
			IAccessManager access)
		{
			Environment = client.Environment;
			Channel = client.Channel;

			Crypto = crypto;
			Access = access;
		}

		public async Task<PublishResponse> Publish<TContent>(TContent message, bool recordHistory = true)
		{
			var msg = JsonConvert.SerializeObject(message);

			if (Channel.Secured && Environment.GrantCapable())
			{
				var grantResponse = await Access.Establish(AccessType.Write);
				if (!grantResponse.Success)
				{
					return new PublishResponse
					{
						Success = false,
						Message = grantResponse.Message
					};
				}
			}

			if (Channel.Encrypted)
			{
				msg = Crypto.Encrypt(Channel.Cipher ?? Environment.CipherKey, msg);
				msg = JsonConvert.SerializeObject(msg);
			}

			var signature = "0";
			if (!string.IsNullOrWhiteSpace(Environment.SecretKey))
			{
				var uri = Environment.PublishKey.AppendPathSegments(
					Environment.SubscribeKey,
					Environment.SecretKey,
					Channel.Name,
					msg);
				signature = Crypto.Hash(uri, HashAlgorithm.Md5);
			}

			var requestUrl = Environment.Host
				.AppendPathSegment("publish")
				.AppendPathSegments(Environment.PublishKey, Environment.SubscribeKey)
				.AppendPathSegment(signature)
				.AppendPathSegment(Channel.Name)
				.AppendPathSegment("0") // "callback" according to pn api - not sure what this is for, but always "0" from PubnubCore.cs
				.AppendPathSegment(msg)
				.SetQueryParam("uuid", Environment.SessionUuid);

			if (!string.IsNullOrWhiteSpace(Environment.AuthenticationKey))
			{
				requestUrl.SetQueryParam("auth", Environment.AuthenticationKey);
			}
			if (!recordHistory)
			{
				requestUrl.SetQueryParam("store", "0");
			}

		    var httpResponse = await requestUrl.GetAsync()
		        .ProcessResponse();

			return await HandleResponse(httpResponse);
		}

		private async Task<PublishResponse> HandleResponse(HttpResponseMessage response)
		{
		    if (response.IsSuccessStatusCode)
		    {
		        var content = await Task.FromResult(response)
		            .ReceiveJson<JToken>();
		        return new PublishResponse
		        {
		            Success = content[0].Value<bool>(),
		            Message = content[1].Value<string>(),
		            Sent = content[2].Value<long>()
		        };
		    }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var content = await Task.FromResult(response)
                    .ReceiveJson<PubNubForbiddenResponse>();
                return new PublishResponse
                {
                    Success = false,
                    Message = content.Message
                };
            }

            return new PublishResponse
            {
                Success = false,
                Message = await Task.FromResult(response)
                    .ReceiveString()
            };
		}
	}
}