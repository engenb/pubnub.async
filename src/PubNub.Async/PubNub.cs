using System;
using PubNub.Async.Configuration;

namespace PubNub.Async
{
	public class PubNub
	{
		private static readonly object SettingsLock = new object();

		private static Lazy<IPubNubSettings> _settings;

		internal static Lazy<IPubNubSettings> Settings
		{
			get { return _settings ?? (_settings = new Lazy<IPubNubSettings>(() => new DefaultPubNubSettings())); }
			set { _settings = value; }
		}

		public static IPubNubSettings GlobalSettings => Settings.Value;

		public static void Configure(Action<IPubNubSettings> configureSettings)
		{
			lock (SettingsLock)
			{
				configureSettings(GlobalSettings);
			}
		}
	}
}