using System;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;

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
		string PublishKey { get; set; }
		string SubscribeKey { get; set; }
		string SecretKey { get; set; }
		string CipherKey { get; set; }

		Func<ICryptoService> CryptoFactory { get; }
		Func<IPubNubClient, IHistoryService> HistoryFactory { get; }

		void Reset();
		IPubNubSettings Clone();
	}
}