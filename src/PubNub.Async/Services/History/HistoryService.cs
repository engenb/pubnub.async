using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.History;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;

namespace PubNub.Async.Services.History
{
	public class HistoryService : IHistoryService
	{
		private ICryptoService Crypto { get; }
		private IAccessManager Access { get; }

		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

		public HistoryService(IPubNubClient client, ICryptoService crypto, IAccessManager access)
		{
			Crypto = crypto;
			Access = access;

			Environment = client.Environment;
			Channel = client.Channel;
		}

		public async Task<HistoryResponse<TContent>> History<TContent>(
			long? first = null,
			long? last = null,
			int? count = null,
			HistoryOrder order = HistoryOrder.Reverse,
			bool includeTime = true)
		{
			var reverse = order == HistoryOrder.Reverse;

			var batch = await FetchHistory<TContent>(first, last, count, reverse, includeTime);
			if (reverse && batch?.Messages != null)
			{
				batch.Messages = batch.Messages.Reverse().ToArray();
			}
			var responseMessageCount = batch?.Messages?.Length;
			if (count > 100 && responseMessageCount == 100) //recurse
			{
				var nextBatchCount = count - responseMessageCount;
				var nextBatchFirst = reverse ? batch.Newest : batch.Oldest;

				var nextBatch = await History<TContent>(nextBatchFirst, last, nextBatchCount, order, includeTime);
				
				batch.Messages = nextBatch.Messages
					.Union(batch.Messages)
					.ToArray();
				batch.Oldest = reverse ? batch.Oldest : nextBatch.Oldest;
				batch.Newest = reverse ? nextBatch.Newest : batch.Newest;
			}
			return batch;
		}

		private async Task<HistoryResponse<TContent>> FetchHistory<TContent>(
			long? first,
			long? last,
			int? count,
			bool reverse,
			bool includeTime)
		{
			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "history")
				.AppendPathSegments("sub-key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.SetQueryParam("uuid", Environment.SessionUuid);

			// pubnub's api will, at most and by default, return 100 records.
			// no need to provide this value if count >= 100
			if (count.HasValue && count.Value < 100)
			{
				requestUrl.SetQueryParam("count", count);
			}
			if (includeTime)
			{
				requestUrl.SetQueryParam("include_token", includeTime);
			}
			if (reverse)
			{
				requestUrl.SetQueryParam("reverse", reverse);
			}
			if (first.HasValue && first > -1)
			{
				requestUrl.SetQueryParam("start", first);
			}
			if (last.HasValue && last > -1)
			{
				requestUrl.SetQueryParam("end", last);
			}
			if (!string.IsNullOrWhiteSpace(Environment.AuthenticationKey))
			{
				requestUrl.SetQueryParam("auth", Environment.AuthenticationKey);
			}
			var rawResponse = await requestUrl.GetAsync()
				.ProcessResponse()
				.ReceiveString();

			return DeserializeResponse<TContent>(Channel, rawResponse, includeTime);
		}

		private HistoryResponse<TContent> DeserializeResponse<TContent>(Channel channel, string rawResponse, bool includeTime)
		{
			if (string.IsNullOrWhiteSpace(rawResponse))
			{
				//TODO: error
				return null;
			}

			var array = JArray.Parse(rawResponse);
			if (!array.HasValues || array.Count != 3)
			{
				//TODO: error
				return null;
			}

			var messages = array[0];
			var start = array[1].Value<long>();
			var end = array[2].Value<long>();

			if (start == 0 && end == 0 && messages.Count() == 1)
			{
				// we probably have an error
				return new HistoryResponse<TContent>
				{
					Error = messages[0].Value<string>()
				};
			}

			return new HistoryResponse<TContent>
			{
				Oldest = start,
				Newest = end,
				Messages = messages.Children()
					.Select(x => channel.Encrypted
						? Decrypt<TContent>(x, channel.Cipher ?? Environment.CipherKey, includeTime)
						: DeserializeRecord<TContent>(x, includeTime))
					.ToArray()
			};
		}

		private HistoryMessage<TContent> DeserializeRecord<TContent>(JToken historyRecord, bool includeTime)
		{
			if (includeTime)
			{
				return historyRecord.ToObject<HistoryMessage<TContent>>();
			}

			return new HistoryMessage<TContent>
			{
				Content = historyRecord.ToObject<TContent>()
			};
		} 

		private HistoryMessage<TContent> Decrypt<TContent>(JToken historyRecord, string cipherKey, bool includeTime)
		{
			var encryptedRecord = DeserializeRecord<string>(historyRecord, includeTime);
			var encryptedContent = encryptedRecord.Content;
			
			var decryptedContent = Crypto.Decrypt(cipherKey, encryptedContent);
			
			return new HistoryMessage<TContent>
			{
				Sent = encryptedRecord.Sent,
				Content = JsonConvert.DeserializeObject<TContent>(decryptedContent)
			};
		}
	}
}