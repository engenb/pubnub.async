using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Presence.Models;
using PubNub.Async.Services.Access;

namespace PubNub.Async.Presence.Services
{
	public class PresenceService : IPresenceService
	{
		private IAccessManager Access { get; }

		private IPubNubEnvironment Environment { get; }
		private Channel Channel { get; }

		public PresenceService(IPubNubClient client, IAccessManager access)
		{
			Environment = client.Environment;
			Channel = client.Channel;

			Access = access;
		}

		public async Task<SessionStateResponse<TState>> SessionState<TState>()
			where TState : class
		{
			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.AppendPathSegments("uuid", Environment.SessionUuid);

			await EstablishAccess(requestUrl);

			var response = await requestUrl
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
			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.AppendPathSegments("uuid", Environment.SessionUuid)
				.AppendPathSegment("data")
				.SetQueryParam("state", JsonConvert.SerializeObject(state));

			await EstablishAccess(requestUrl);

			var response = await requestUrl
				.GetAsync()
				.ProcessResponse()
				.ReceiveJson<PubNubStateResponse<TState>>();

			return new SessionStateResponse<TState>
			{
				Success = response?.Status == HttpStatusCode.OK,
				State = response?.Payload
			};
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

			await EstablishAccess(requestUrl);

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

		public async Task<SubscriptionsResponse> Subscriptions()
		{
			var requestUrl = Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("uuid", Environment.SessionUuid);
			
			await EstablishAccess(requestUrl);

			var response = await requestUrl
				.GetAsync()
				.ProcessResponse()
				.ReceiveJson<PubNubSubscriptionsResponse>();

			return new SubscriptionsResponse
			{
				Success = response.Status == HttpStatusCode.OK,
				Message = response.Message,
				Channels = response.Payload?.Channels
			};
		}

		private async Task EstablishAccess(Url requestUrl)
		{
			if (Channel.Secured)
			{
				var grantResponse = await Access.Establish(AccessType.Read);
				if (!grantResponse.Success)
				{
					//TODO: throw exception?
				}

				requestUrl.SetQueryParam("auth", Environment.AuthenticationKey);
			}
		}
	}
}