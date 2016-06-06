using System;
using Autofac;
using PubNub.Async.Configuration;

namespace PubNub.Async.Autofac
{
	public class PubNubSettingsBootstrapper : IStartable
	{
		public PubNubSettingsBootstrapper(Lazy<IPubNubSettings> settingsFactory)
		{
			SettingsFactory = settingsFactory;
		}

		private Lazy<IPubNubSettings> SettingsFactory { get; }

		public void Start()
		{
			PubNub.Settings = SettingsFactory;
		}
	}
}