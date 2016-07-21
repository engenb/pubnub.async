using System;

namespace PubNub.Async.Configuration
{
	public abstract class AbstractPubNubEnvironment : IPubNubEnvironment
	{
		protected AbstractPubNubEnvironment()
		{
			Reset();
		}
        
		public bool SslEnabled { get; set; }
		public string Origin { get; set; }
		public string Host => $"{(SslEnabled ? "https://" : "http://")}{Origin}";
        
		public string SessionUuid { get; set; }

		public string AuthenticationKey { get; set; }
		public int? MinutesToTimeout { get; set; }

		public string PublishKey { get; set; }
		public string SubscribeKey { get; set; }
		public string SecretKey { get; set; }
		public string CipherKey { get; set; }

		public abstract TService Resolve<TService>(IPubNubClient client);
	    public bool GrantCapable()
	    {
	        return !string.IsNullOrWhiteSpace(PublishKey)
	               && !string.IsNullOrWhiteSpace(SubscribeKey)
	               && !string.IsNullOrWhiteSpace(SecretKey);
	    }

	    public void Reset()
		{
			Origin = "pubsub.pubnub.com";

			SessionUuid = Guid.NewGuid().ToString();
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