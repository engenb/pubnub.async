using System.Threading.Tasks;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;

namespace PubNub.Async.Services.Access
{
	public interface IAccessRegistry
	{
		Task Register(Channel channel, string authenticationKey, AccessGrantResponse response);
		Task<AccessGrantResponse> Registration(Channel channel, string authenticationKey);
		bool InForce(Channel channel, string authenticationKey);
		void Unregister(Channel channel, string authenticationKey);
	}
}
