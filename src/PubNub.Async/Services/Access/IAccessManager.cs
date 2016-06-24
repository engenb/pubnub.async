using System.Threading.Tasks;
using PubNub.Async.Models.Access;

namespace PubNub.Async.Services.Access
{
	public interface IAccessManager
	{
		Task<GrantResponse> Establish(AccessType access);
		Task Revoke(AccessType access);
	}
}
