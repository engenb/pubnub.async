using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Subscribe;

namespace PubNub.Async.Models.Subscribe
{
	public abstract class Subscription
	{
		public IPubNubEnvironment Environment { get; }
		public Channel Channel { get; }

		protected Subscription(IPubNubEnvironment environment, Channel channel)
		{
			Environment = environment.Clone();
			Channel = channel.Clone();
		}

		public void OnMessageReceived(PubNubSubscribeResponseMessage message)
		{
			if (Environment.SubscribeKey == message.SubscribeKey && Channel.Name == message.Channel)
			{
				ProcessMessage(message);
			}
		}

		protected abstract void ProcessMessage(PubNubSubscribeResponseMessage message);

		public override bool Equals(object obj)
		{
			if (!(obj is Subscription))
			{
				return false;
			}
			var that = (Subscription)obj;
			return this.Environment.SubscribeKey == that.Environment.SubscribeKey
				   && this.Environment.AuthenticationKey == that.Environment.AuthenticationKey
				   && this.Channel == that.Channel;
		}

		public override int GetHashCode()
		{
			var hash = 17;

			hash = hash * 23 + Environment.SubscribeKey.GetHashCode();
			hash = hash * 23 + Environment.AuthenticationKey.GetHashCode();
			hash = hash * 23 + Channel.GetHashCode();

			return hash;
		}
	}

	public class Subscription<TMessage> : Subscription
	{
		private ICryptoService Crypto { get; }

		public event MessageReceivedHandler<TMessage> MessageReceived;

		public Subscription(
			IPubNubEnvironment environment,
			Channel channel,
			ICryptoService crypto) :
			base(environment, channel)
		{
			Crypto = crypto;
		}

		public void Add(MessageReceivedHandler<TMessage> handler)
		{
			MessageReceived += handler;
		}

		public void Remove(MessageReceivedHandler<TMessage> handler)
		{
			MessageReceived -= handler;
		}

		protected override void ProcessMessage(PubNubSubscribeResponseMessage message)
		{
			if (MessageReceived != null)
			{
				JToken decryptedMsgJson = null;
				var msg = default(TMessage);

				try
				{
					if (Channel.Encrypted)
					{
						var decrypted = Crypto.Decrypt(Channel.Cipher, message.Data.ToObject<string>());
						decryptedMsgJson = JToken.Parse(decrypted);
					}
					else
					{
						decryptedMsgJson = message.Data;
					}
					msg = decryptedMsgJson.ToObject<TMessage>();
				}
				catch (Exception)
				{
					//TODO: warn of decryption failure (wrong cipher?) conversion failure (wrong model?)
				}

				MessageReceived(new MessageReceivedEventArgs<TMessage>(
				   message.SubscribeKey,
				   message.Channel,
				   message.SessionUuid,
				   message.Processed.TimeToken,
				   message.Data,
				   decryptedMsgJson,
				   msg));
			}
		}
	}

}
