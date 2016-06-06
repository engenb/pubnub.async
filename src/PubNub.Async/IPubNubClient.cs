using System;
using PubNub.Async.Configuration;
using PubNub.Async.Models.Channel;

namespace PubNub.Async
{
	public interface IPubNubClient
	{
		Channel Channel { get; }
		IPubNubSettings Settings { get; }

		IPubNubClient ConfigureClient(Action<IPubNubSettings> action);
		IPubNubClient Encrypted();
		IPubNubClient EncryptedWith(string cipher);
	}
}