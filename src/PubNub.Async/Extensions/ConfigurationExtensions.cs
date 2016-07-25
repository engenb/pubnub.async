using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models;

namespace PubNub.Async.Extensions
{
	public static class ConfigurationExtensions
	{
		public static IPubNubClient ConfigurePubNub(this string channel, Action<IPubNubEnvironment> action)
		{
			return new PubNubClient(channel)
				.ConfigurePubNub(action);
		}

		public static IPubNubClient ConfigurePubNub(this Channel channel, Action<IPubNubEnvironment> action)
		{
			return new PubNubClient(channel)
				.ConfigurePubNub(action);
		}
	}
}