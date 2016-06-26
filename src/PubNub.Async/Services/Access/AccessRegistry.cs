using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;

namespace PubNub.Async.Services.Access
{
	public class AccessRegistry : IAccessRegistry
	{
		//TODO: this registry could potentially get very large/bloated
		//TODO: maybe make this more robust with some sort of cache impl
		private IDictionary<string, AccessRegistration> Registry { get; }
		private IDictionary<string, byte[]> ResponseRegistry { get; }

		public AccessRegistry()
		{
			Registry = new ConcurrentDictionary<string, AccessRegistration>();
			ResponseRegistry = new ConcurrentDictionary<string, byte[]>();

			//TODO: launch thread to clean registry (expired grant responses)
		}

		public async Task Register(Channel channel, string authenticationKey, GrantResponse grant)
		{
			var key = KeyFor(channel, authenticationKey);
			var expiration = DateTime.UtcNow.AddMinutes(grant.MinutesToExpire).Ticks;

			Registry[key] = new AccessRegistration
			{
				ReadExpires = grant.Access.GrantsRead() ? expiration : (long?) null,
				WriteExpires = grant.Access.GrantsWrite() ? expiration : (long?) null
			};
			ResponseRegistry[key] = await Compress(JsonConvert.SerializeObject(grant));
		}

		public async Task<GrantResponse> CachedRegistration(Channel channel, string authenticationKey)
		{
			var key = KeyFor(channel, authenticationKey);
			return Registry.ContainsKey(key)
				? JsonConvert.DeserializeObject<GrantResponse>(await Decompress(ResponseRegistry[key]))
				: null;
		}

		public bool Granted(Channel channel, string authenticationKey, AccessType access)
		{
			var key = KeyFor(channel, authenticationKey);
			return Registry.ContainsKey(key) && Registry[key].AccessValid(access);
		}

		public void Unregister(Channel channel, string authenticationKey)
		{
			if (channel == null) throw new ArgumentNullException(nameof(channel));
			if (string.IsNullOrWhiteSpace(authenticationKey)) throw new ArgumentNullException(nameof(authenticationKey));

			var key = KeyFor(channel, authenticationKey);
			if (Registry.ContainsKey(key))
			{
				Registry.Remove(key);
			}
			if (ResponseRegistry.ContainsKey(key))
			{
				ResponseRegistry.Remove(key);
			}
		}

		private static string KeyFor(Channel channel, string authenticationKey)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{channel.Name}{authenticationKey}"));
		}

		private static async Task<byte[]> Compress(string rawResponse)
		{
			using (var outStream = new MemoryStream())
			{
				using (var gzip = new GZipStream(outStream, CompressionMode.Compress))
				using (var inStream = new MemoryStream(Encoding.UTF8.GetBytes(rawResponse)))
				{
					await inStream.CopyToAsync(gzip);
				}
				return outStream.ToArray();
			}
		}

		private static async Task<string> Decompress(byte[] compressedResponse)
		{
			using (var inStream = new MemoryStream(compressedResponse))
			using (var gzip = new GZipStream(inStream, CompressionMode.Decompress))
			using (var outStream = new MemoryStream())
			{
				await gzip.CopyToAsync(outStream);
				var outBytes = outStream.ToArray();
				return Encoding.UTF8.GetString(outBytes, 0, outBytes.Length);
			}
		}
	}
}