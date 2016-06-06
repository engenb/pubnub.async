using System;
using PubNub.Async.Configuration;

namespace PubNub.Async.Testing
{
	public class PubNubTest : IDisposable
	{
		public PubNubTest() : this(new TestablePubNubSettings())
		{
		}

		public PubNubTest(IPubNubSettings settings)
		{
			PubNub.Settings = new Lazy<IPubNubSettings>(() => settings);
		}

		public void Dispose()
		{
			PubNub.GlobalSettings.Reset();
		}
	}
}