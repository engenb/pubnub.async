using System;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Services.Publish;

namespace PubNub.Async.Configuration
{
	public interface IPubNubSettings
	{
		string SdkVersion { get; set; }

		bool SslEnabled { get; set; }
		string Origin { get; set; }
		string Host { get; }

		string SessionUuid { get; set; }
		string AuthenticationKey { get; set; }
		int? MinutesToTimeout { get; set; }

		string PublishKey { get; set; }
		string SubscribeKey { get; set; }
		string SecretKey { get; set; }

		string CipherKey { get; set; }

		Func<ICryptoService> CryptoFactory { get; }
		Func<IPubNubClient, IAccessManager> AccessFactory { get; }
		Func<IPubNubClient, IHistoryService> HistoryFactory { get; }
		Func<IPubNubClient, IPublishService> PublishFactory { get; }

		void Reset();
		IPubNubSettings Clone();
	}
}