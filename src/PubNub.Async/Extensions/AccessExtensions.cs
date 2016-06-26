using System.Threading.Tasks;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Services.Access;

namespace PubNub.Async.Extensions
{
	public static class AccessExtensions
	{
		public static Task<GrantResponse> Grant(this string channel, AccessType access)
		{
			return new PubNubClient(channel)
				.Grant(access);
		}

		public static Task<GrantResponse> Grant(this Channel channel, AccessType access)
		{
			return new PubNubClient(channel)
				.Grant(access);
		}

		public static async Task<GrantResponse> Grant(this IPubNubClient client, AccessType access)
		{
			return await PubNub.Environment
				.Resolve<IAccessManager>(client)
				.Establish(access)
				.ConfigureAwait(false);
		}
	}
}