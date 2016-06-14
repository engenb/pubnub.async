using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLCrypto;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;

namespace PubNub.Async.Services.Publish
{
	public class PublishService : IPublishService
	{
		private ICryptoService Crypto { get; }
		private IAccessManager Access { get; }

		private IPubNubSettings Settings { get; }
		private Channel Channel { get; }

		public PublishService(
			IPubNubClient client,
			ICryptoService crypto,
			IAccessManager access)
		{
			Settings = client.Settings;
			Channel = client.Channel;

			Crypto = crypto;
			Access = access;
		}

		public async Task<PublishResponse> Publish<TContent>(TContent message, bool recordHistory = true)
		{
			var msg = JsonConvert.SerializeObject(message);
			
			if (Channel.Secured)
			{
				var grantResponse = await Access.Establish(AccessType.Write);
				//if (!grantResponse.Success)
				{
					//TODO: do something
				}
			}

			if (Channel.Encrypted)
			{
				msg = Crypto.Encrypt(Channel.Cipher ?? Settings.CipherKey, msg);
				msg = JsonConvert.SerializeObject(msg);
			}

			var signature = "0";
			if (!string.IsNullOrWhiteSpace(Settings.SecretKey))
			{
				var uri = Settings.PublishKey.AppendPathSegments(
					Settings.SubscribeKey,
					Settings.SecretKey,
					Channel.Name,
					msg);
				signature = Crypto.Hash(uri, HashAlgorithm.Md5);
			}

			var requestUrl = Settings.Host
				.AppendPathSegment("publish")
				.AppendPathSegments(Settings.PublishKey, Settings.SubscribeKey)
				.AppendPathSegment(signature)
				.AppendPathSegment(Channel.Name)
				.AppendPathSegment("0") // "callback" according to pn api - not sure what this is for, but always "0" from PubnubCore.cs
				.AppendPathSegment(msg)
				.SetQueryParam("uuid", Settings.SessionUuid);

			if (!string.IsNullOrWhiteSpace(Settings.AuthenticationKey))
			{
				requestUrl.SetQueryParam("auth", Settings.AuthenticationKey);
			}
			if (!recordHistory)
			{
				requestUrl.SetQueryParam("store", "0");
			}
			
			var rawResponse = await requestUrl.GetAsync()
				.ProcessResponse()
				.ReceiveString();

			return DeserializeResponse(rawResponse);
		}

		private PublishResponse DeserializeResponse(string rawResponse)
		{
			var array = JArray.Parse(rawResponse);
			if (!array.HasValues || array.Count != 3)
			{
				//TODO: error
				return null;
			}

			return new PublishResponse
			{
				Success = array[0].Value<bool>(),
				Message = array[1].Value<string>(),
				Sent = array[2].Value<long>()
			};
		}
	}
}
