using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models.Channel;

namespace PubNub.Async
{
	public class PubNubClient : IPubNubClient
	{
		public IPubNubEnvironment Environment { get; }
		public Channel Channel { get; }
		
		public PubNubClient(string channel) : this(new Channel(channel))
		{
		}

		public PubNubClient(Channel channel)
		{
			Environment = PubNub.Environment.Clone();
			Channel = channel;
		}

		public IPubNubClient ConfigurePubNub(Action<IPubNubEnvironment> action)
		{
			action(Environment);
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
			Environment.MinutesToTimeout = minutesToTimeout;
			return this;
		}

		public IPubNubClient SecuredWith(string authenticationKey, int? minutesToTimeout = null)
		{
			Channel.Secured = true;
			Environment.AuthenticationKey = authenticationKey;
			Environment.MinutesToTimeout = minutesToTimeout;
			return this;
		}
	}
}