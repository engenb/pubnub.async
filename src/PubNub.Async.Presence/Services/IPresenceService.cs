using System.Threading.Tasks;
using PubNub.Async.Presence.Models;

namespace PubNub.Async.Presence.Services
{
	public interface IPresenceService
	{
		Task<PresenceResponse<TState>> GetState<TState>()
			where TState : class;

		Task<PresenceResponse<TState>> SetState<TState>(TState state)
			where TState : class;
	}
}