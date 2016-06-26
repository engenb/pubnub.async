using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
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

		public async Task<PresenceResponse<TState>> GetState<TState>()
			where TState : class
		{
			var response = await Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", Environment.SubscribeKey)
				.AppendPathSegments("channel", Channel.Name)
				.AppendPathSegments("uuid", Environment.SessionUuid)
				.GetAsync()
				.ProcessResponse()
				.ReceiveJson<StateResponse<TState>>();

			return new PresenceResponse<TState>
			{
				Success = response.Status == HttpStatusCode.OK,
				State = response.Payload
			};
		}

		public async Task<PresenceResponse<TState>> SetState<TState>(TState state)
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
				.ReceiveJson<StateResponse<TState>>();

			return ConstructResponse(response);
		}

		private PresenceResponse<TState> ConstructResponse<TState>(StateResponse<TState> response)
			where TState : class
		{
			return new PresenceResponse<TState>
			{
				Success = response?.Status == HttpStatusCode.OK,
				State = response?.Payload
			};
		}
	}
}