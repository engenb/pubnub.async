using PubNub.Async.Models;

namespace PubNub.Async.Extensions
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

		public static IPubNubClient Secured(this string channel, int? minutesToTimeout = null)
		{
			return new PubNubClient(channel).Secured(minutesToTimeout);
		}

		public static IPubNubClient Secured(this Channel channel, int? minutesToTimeout = null)
		{
			return new PubNubClient(channel).Secured(minutesToTimeout);
		}

		public static IPubNubClient SecuredWith(this string channel, string authenticationKey, int? minutesToTimeout = null)
		{
			return new PubNubClient(channel).SecuredWith(authenticationKey, minutesToTimeout);
		}

		public static IPubNubClient SecuredWith(this Channel channel, string authenticationKey, int? minutesToTimeout = null)
		{
			return new PubNubClient(channel).SecuredWith(authenticationKey, minutesToTimeout);
		}
	}
}