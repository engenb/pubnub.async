using System.Threading.Tasks;
using PubNub.Async.Presence.Models;

namespace PubNub.Async.Presence.Services
{
	public interface IPresenceService
	{
		Task<SessionStateResponse<TState>> SessionState<TState>()
			where TState : class;

		Task<SessionStateResponse<TState>> SessionState<TState>(TState state)
			where TState : class;
		
		Task<SubscribersResponse<TState>> Subscribers<TState>(bool includeSessionState = false, bool includeUuids = true)
			where TState : class;

		Task<SubscriptionsResponse> Subscriptions();
	}
}