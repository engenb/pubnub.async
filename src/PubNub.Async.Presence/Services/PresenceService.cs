using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Channel;
using PubNub.Async.Presence.Models;

namespace PubNub.Async.Presence.Services
{
	public class PresenceService : IPresenceService
	{
		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

		public PresenceService(IPubNubClient client)
		{
			Environment = client.Environment;
			Channel = client.Channel;
		}

		public async Task<SessionStateResponse<TState>> SessionState<TState>()
			where TState : class
		{
			var response = await Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.AppendPathSegments("uuid", Environment.SessionUuid)
				.GetAsync()
				.ProcessResponse()
				.ReceiveJson<PubNubStateResponse<TState>>();

			return new SessionStateResponse<TState>
			{
				Success = response.Status == HttpStatusCode.OK,
				State = response.Payload
			};
		}

		public async Task<SessionStateResponse<TState>> SessionState<TState>(TState state)
			where TState : class
		{
			var response = await Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.AppendPathSegments("uuid", Environment.SessionUuid)
				.AppendPathSegment("data")
				.SetQueryParam("state", JsonConvert.SerializeObject(state))
				.GetAsync()
				.ProcessResponse()
				.ReceiveJson<PubNubStateResponse<TState>>();

			return ConstructResponse(response);
		}

		public async Task<SubscribersResponse<TState>> Subscribers<TState>(bool includeSessionState = false, bool includeUuids = true)
			where TState : class
		{
			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name);

			if (!includeUuids)
			{
				requestUrl.SetQueryParam("disable_uuids", 1);
			}
			if (includeSessionState)
			{
				requestUrl.SetQueryParam("state", 1);
			}

			var response = await requestUrl
				.GetAsync()
				.ProcessResponse()
				.ReceiveString();

			if (includeSessionState)
			{
				var subscriberStates = JsonConvert.DeserializeObject<PubNubSubscribersResponse<TState>>(response);
				return new SubscribersResponse<TState>
				{
					Success = subscriberStates.Status == HttpStatusCode.OK,
					Message = subscriberStates.Message,
					Occupancy = subscriberStates.Occupancy,
					Subscribers = subscriberStates.Subscribers
				};
			}

			var subscriberUuids = JsonConvert.DeserializeObject<PubNubSubscriberUuidsResponse>(response);
			return new SubscribersResponse<TState>
			{
				Success = subscriberUuids.Status == HttpStatusCode.OK,
				Message = subscriberUuids.Message,
				Occupancy = subscriberUuids.Occupancy,
				Subscribers = subscriberUuids.Subscribers?
					.Select(x => new Subscriber<TState> {Uuid = x})
					.ToArray()
			};
		}

		private SessionStateResponse<TState> ConstructResponse<TState>(PubNubStateResponse<TState> response)
			where TState : class
		{
			return new SessionStateResponse<TState>
			{
				Success = response?.Status == HttpStatusCode.OK,
				State = response?.Payload
			};
		}
	}
}