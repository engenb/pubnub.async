namespace PubNub.Async.Configuration
{
	public interface IPubNubEnvironment
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

		TService Resolve<TService>(IPubNubClient client);

		void Reset();
		IPubNubEnvironment Clone();
	}
}