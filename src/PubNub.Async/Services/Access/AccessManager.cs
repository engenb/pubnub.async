using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using PCLCrypto;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using static PCLCrypto.WinRTCrypto;

namespace PubNub.Async.Services.Access
{
	public class AccessManager : IAccessManager
	{
		private IAccessRegistry AccessRegistry { get; }

		private IPubNubSettings Settings { get; }
		private Channel Channel { get; }

		public AccessManager(IPubNubClient client, IAccessRegistry accessRegistry)
		{
			AccessRegistry = accessRegistry;

			Settings = client.Settings;
			Channel = client.Channel;
		}

		public async Task<AccessGrantResponse> Establish(AccessType access)
		{
			//TODO - handle r/w/rw
			access = AccessType.ReadWrite;

			if (string.IsNullOrWhiteSpace(Settings.SecretKey))
			{
				throw new InvalidOperationException("PubNubClient must be configured with secret key in order to establish access");
			}
			if (!Channel.Secured)
			{
				//TODO: warn grant on unsecured channel
			}

			// if access grant is in force, return cached result
			if (AccessRegistry.Granted(Channel, Settings.AuthenticationKey))
			{
				return await AccessRegistry.Registration(Channel, Settings.AuthenticationKey);
			}

			// I have experimented with this a bit, and the ORDER of params in the url appears to matter...
			var requestUrl = Settings.Host
				.AppendPathSegments("v1", "auth", "grant")
				.AppendPathSegments("sub-key", Settings.SubscribeKey);

			if (!string.IsNullOrWhiteSpace(Settings.AuthenticationKey))
			{
				requestUrl.SetQueryParam("auth", Settings.AuthenticationKey);
			}

			requestUrl
				.SetQueryParam("channel", Channel.Name)
				//.SetQueryParam("pnsdk", Settings.SdkVersion)
				.SetQueryParam("r", Convert.ToInt32(access.GrantsRead()))
				.SetQueryParam("timestamp", SecondsSinceEpoch(DateTime.UtcNow));

			if (Settings.MinutesToTimeout != null)
			{
				requestUrl.SetQueryParam("ttl", Settings.MinutesToTimeout.Value);
			}

			requestUrl
				.SetQueryParam("uuid", Settings.SessionUuid)
				.SetQueryParam("w", Convert.ToInt32(access.GrantsWrite()));

			//encode signature
			var signature = string.Join("\n",
				Settings.SubscribeKey,
				Settings.PublishKey,
				"grant",
				requestUrl.Query);
			requestUrl.SetQueryParam("signature", Sign(Settings.SecretKey, signature), true);

			var responseMessage = await requestUrl.GetAsync();

			var response = await DeserializeResponse(responseMessage);

			//TODO: handle error

			await AccessRegistry.Register(Channel, Settings.AuthenticationKey, response);
			return response;
		}

		public Task Revoke(AccessType access)
		{
			throw new System.NotImplementedException();
		}

		private static long SecondsSinceEpoch(DateTime utcNow)
		{
			var timeSpan = utcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64(timeSpan.TotalSeconds);
		}

		private string Sign(string secret, string signature)
		{
			var provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha256);
			var signatureBuffer = CryptographicBuffer.ConvertStringToBinary(signature, Encoding.UTF8);
			var keyBuffer = CryptographicBuffer.ConvertStringToBinary(secret, Encoding.UTF8);

			var key = provider.CreateKey(keyBuffer);

			//sign the key and signature together
			var signed = CryptographicEngine.Sign(key, signatureBuffer);

			return Convert.ToBase64String(signed)
				.Replace('+', '-')
				.Replace('/', '_');
		}

		private async Task<AccessGrantResponse> DeserializeResponse(HttpResponseMessage response)
		{
			var rawContent = await response
				.StripCharsetQuotes()
				.Content
				.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<AccessGrantResponse>(rawContent);
		}
	}
}
