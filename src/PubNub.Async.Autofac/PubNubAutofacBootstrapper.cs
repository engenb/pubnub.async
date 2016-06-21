using System;
using Autofac;
using PubNub.Async.Configuration;

namespace PubNub.Async.Autofac
{
	public class PubNubAutofacBootstrapper : IStartable
	{
		private Lazy<IPubNubEnvironment> AutofacLazyEnvironment { get; }

		public PubNubAutofacBootstrapper(Lazy<IPubNubEnvironment> environment)
		{
			AutofacLazyEnvironment = environment;
		}

		public void Start()
		{
			var oldLazyEnvironment = PubNub.InternalEnvironment;
			PubNub.InternalEnvironment = AutofacLazyEnvironment;

			// if the old environment was previously evaluated/configured...
			if (oldLazyEnvironment.IsValueCreated)
			{
				var oldEnvironment = oldLazyEnvironment.Value;
				// ...then copy the old settings to the new environment
				PubNub.Configure(c =>
				{
					c.AuthenticationKey = oldEnvironment.AuthenticationKey;
					c.CipherKey = oldEnvironment.CipherKey;
					c.MinutesToTimeout = oldEnvironment.MinutesToTimeout;
					c.Origin = oldEnvironment.Origin;
					c.PublishKey = oldEnvironment.PublishKey;
					c.SdkVersion = oldEnvironment.SdkVersion;
					c.SecretKey = oldEnvironment.SecretKey;
					c.SessionUuid = oldEnvironment.SessionUuid;
					c.SslEnabled = oldEnvironment.SslEnabled;
					c.SubscribeKey = oldEnvironment.SubscribeKey;
				});
			}
		}
	}
}
