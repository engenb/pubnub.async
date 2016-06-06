using System;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;

namespace PubNub.Async.Configuration
{
	public abstract class AbstractPubNubSettings : IPubNubSettings
	{
		protected AbstractPubNubSettings()
		{
			Reset();
		}

		public string SdkVersion { get; set; }
		public bool SslEnabled { get; set; }
		public string Origin { get; set; }
		public string Host => $"{(SslEnabled ? "https://" : "http://")}{Origin}";

		public string SessionUuid { get; set; }
		public string AuthenticationKey { get; set; }

		public string PublishKey { get; set; }
		public string SubscribeKey { get; set; }
		public string SecretKey { get; set; }
		public string CipherKey { get; set; }

		public abstract Func<ICryptoService> CryptoFactory { get; }
		public abstract Func<IPubNubClient, IHistoryService> HistoryFactory { get; }

		public void Reset()
		{
			SdkVersion = "PubNub-CSharp-.NET/3.7.1";
			Origin = "pubsub.pubnub.com";

			SessionUuid = null;
			AuthenticationKey = null;

			PublishKey = null;
			SubscribeKey = null;
			SecretKey = null;
			CipherKey = null;
			SslEnabled = true;
		}

		public IPubNubSettings Clone()
		{
			return (IPubNubSettings) MemberwiseClone();
		}
	}
}