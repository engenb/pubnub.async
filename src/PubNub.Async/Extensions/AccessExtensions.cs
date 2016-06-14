using System.Threading.Tasks;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Services.Access;

namespace PubNub.Async.Extensions
{
	public static class AccessExtensions
	{
		public static async Task<AccessGrantResponse> Grant(this string channel, AccessType access)
		{
			return await new PubNubClient(channel)
				.Grant(access);
		}

		public static async Task<AccessGrantResponse> Grant(this Channel channel, AccessType access)
		{
			return await new PubNubClient(channel)
				.Grant(access);
		}

		public static async Task<AccessGrantResponse> Grant(this IPubNubClient client, AccessType access)
		{
			return await PubNub.GlobalSettings
				.AccessFactory(client)
				.Establish(access);
		}
	}
}
