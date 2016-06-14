using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models.Channel;

namespace PubNub.Async
{
	public class PubNubClient : IPubNubClient
	{
		public IPubNubSettings Settings { get; }
		public Channel Channel { get; }
		
		public PubNubClient(string channel) : this(new Channel(channel))
		{
		}

		public PubNubClient(Channel channel)
		{
			Settings = PubNub.GlobalSettings.Clone();
			Channel = channel;
		}

		public IPubNubClient ConfigurePubNub(Action<IPubNubSettings> action)
		{
			action(Settings);
			return this;
		}

		public IPubNubClient Encrypted()
		{
			Channel.Encrypted = true;
			return this;
		}

		public IPubNubClient EncryptedWith(string cipher)
		{
			Channel.Encrypted = true;
			Channel.Cipher = cipher;
			return this;
		}

		public IPubNubClient Secured(int? minutesToTimeout = null)
		{
			Channel.Secured = true;
			Settings.MinutesToTimeout = minutesToTimeout;
			return this;
		}

		public IPubNubClient SecuredWith(string authenticationKey, int? minutesToTimeout = null)
		{
			Channel.Secured = true;
			Settings.AuthenticationKey = authenticationKey;
			Settings.MinutesToTimeout = minutesToTimeout;
			return this;
		}
	}
}