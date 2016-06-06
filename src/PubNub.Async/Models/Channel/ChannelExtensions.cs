namespace PubNub.Async.Models.Channel
{
	public static class ChannelExtensions
	{
		public static IPubNubClient Encrypted(this string channel)
		{
			return new PubNubClient(channel).Encrypted();
		}

		public static IPubNubClient Encrypted(this Channel channel)
		{
			return new PubNubClient(channel).Encrypted();
		}

		public static IPubNubClient EncryptedWith(this string channel, string cipher)
		{
			return new PubNubClient(channel).EncryptedWith(cipher);
		}

		public static IPubNubClient EncryptedWith(this Channel channel, string cipher)
		{
			return new PubNubClient(channel).EncryptedWith(cipher);
		}
	}
}