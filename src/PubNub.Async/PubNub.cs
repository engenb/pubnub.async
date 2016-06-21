using System;
using System.Threading;
using PubNub.Async.Configuration;

namespace PubNub.Async
{
	public class PubNub
	{
		private static readonly object SettingsLock = new object();

		private static Lazy<IPubNubEnvironment> _environment;
		internal static Lazy<IPubNubEnvironment> InternalEnvironment
		{
			get { return _environment ?? (_environment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment(), LazyThreadSafetyMode.ExecutionAndPublication)); }
			set { _environment = value; }
		}

		public static IPubNubEnvironment Environment => InternalEnvironment.Value;

		public static void Configure(Action<IPubNubEnvironment> configureSettings)
		{
			configureSettings(Environment);
		}
	}
}