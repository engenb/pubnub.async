using System;
using PubNub.Async.Models.Channel;

namespace PubNub.Async.Configuration
{
	public static class ConfigurationExtensions
	{
		public static IPubNubClient ConfigurePubNub(this string channel, Action<IPubNubSettings> action)
		{
			return new PubNubClient(channel).ConfigurePubNub(action);
		}

		public static IPubNubClient ConfigurePubNub(this Channel channel, Action<IPubNubSettings> action)
		{
			return new PubNubClient(channel).ConfigurePubNub(action);
		}
	}
}