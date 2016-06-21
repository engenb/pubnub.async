using System;
using PubNub.Async.Configuration;

namespace PubNub.Async.Testing
{
	public class PubNubTest : IDisposable
	{
		public PubNubTest() : this(new TestablePubNubEnvironment())
		{
		}

		public PubNubTest(IPubNubEnvironment environment)
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => environment);
		}

		public void Dispose()
		{
			PubNub.Environment.Reset();
		}
	}
}