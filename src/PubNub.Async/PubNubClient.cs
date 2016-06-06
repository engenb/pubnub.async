using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models.Channel;

namespace PubNub.Async
{
	public class PubNubClient : IPubNubClient
	{
		public PubNubClient(Channel channel)
		{
			Settings = PubNub.GlobalSettings.Clone();
			Channel = channel;
		}

		public PubNubClient(string channel) : this(new Channel(channel))
		{
		}

		public IPubNubSettings Settings { get; }

		public Channel Channel { get; }

		public IPubNubClient ConfigureClient(Action<IPubNubSettings> action)
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
	}
}