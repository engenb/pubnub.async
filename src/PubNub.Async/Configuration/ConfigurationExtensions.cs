using System;
using PubNub.Async.Models.Channel;

namespace PubNub.Async.Configuration
{
	public static class ConfigurationExtensions
	{
		public static IPubNubClient ConfigureClient(this string channel, Action<IPubNubSettings> action)
		{
			return new PubNubClient(channel).ConfigureClient(action);
		}

		public static IPubNubClient ConfigureClient(this Channel channel, Action<IPubNubSettings> action)
		{
			return new PubNubClient(channel).ConfigureClient(action);
		}
	}
}