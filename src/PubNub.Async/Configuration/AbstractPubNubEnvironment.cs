using System;

namespace PubNub.Async.Configuration
{
	public abstract class AbstractPubNubEnvironment : IPubNubEnvironment
	{
		private string _sessionUuid;

		protected AbstractPubNubEnvironment()
		{
			Reset();
		}

		public string SdkVersion { get; set; }
		public bool SslEnabled { get; set; }
		public string Origin { get; set; }
		public string Host => $"{(SslEnabled ? "https://" : "http://")}{Origin}";

		public string SessionUuid
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_sessionUuid))
				{
					_sessionUuid = Guid.NewGuid().ToString();
				}
				return _sessionUuid;
			}
			set { _sessionUuid = value; }
		}

		public string AuthenticationKey { get; set; }
		public int? MinutesToTimeout { get; set; }

		public string PublishKey { get; set; }
		public string SubscribeKey { get; set; }
		public string SecretKey { get; set; }
		public string CipherKey { get; set; }

		public abstract TService Resolve<TService>(IPubNubClient client);

		public void Reset()
		{
			SdkVersion = "PubNub-CSharp-.NET/3.7.1";
			Origin = "pubsub.pubnub.com";

			SessionUuid = null;
			AuthenticationKey = null;
			MinutesToTimeout = null;

			PublishKey = null;
			SubscribeKey = null;
			SecretKey = null;
			CipherKey = null;
			SslEnabled = true;
		}

		public IPubNubEnvironment Clone()
		{
			return (IPubNubEnvironment) MemberwiseClone();
		}
	}
}