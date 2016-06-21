using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models.Channel;

namespace PubNub.Async
{
	public interface IPubNubClient
	{
		Channel Channel { get; }
		IPubNubEnvironment Environment { get; }

		IPubNubClient ConfigurePubNub(Action<IPubNubEnvironment> action);

		IPubNubClient Encrypted();
		IPubNubClient EncryptedWith(string cipher);

		IPubNubClient Secured(int? accessTimeout = null);
		IPubNubClient SecuredWith(string authenticationKey, int? accessTimeout = null);
	}
}