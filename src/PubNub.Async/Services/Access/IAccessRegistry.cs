using System.Threading.Tasks;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;

namespace PubNub.Async.Services.Access
{
	public interface IAccessRegistry
	{
		Task Register(Channel channel, string authenticationKey, GrantResponse grant);
		Task<GrantResponse> CachedRegistration(Channel channel, string authenticationKey);
		bool Granted(Channel channel, string authenticationKey, AccessType access);
		void Unregister(Channel channel, string authenticationKey);
	}
}