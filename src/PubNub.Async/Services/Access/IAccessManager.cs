using System.Threading.Tasks;
using PubNub.Async.Models.Access;

namespace PubNub.Async.Services.Access
{
	public interface IAccessManager
	{
		Task<AccessGrantResponse> Establish(AccessType access);
		Task Revoke(AccessType access);
	}
}
