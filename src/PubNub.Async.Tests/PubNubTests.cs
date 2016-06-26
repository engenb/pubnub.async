using PubNub.Async.Configuration;
using Xunit;

namespace PubNub.Async.Tests
{
	public class PubNubTests
	{
		[Fact]
		public void ConfigurePubNub__Given_ConfigAction__Then_InvokeAction()
		{
			IPubNubEnvironment capturedEnv = null;
			var actionExecuted = false;

			PubNub.Configure(c =>
			{
				actionExecuted = true;
				capturedEnv = c;
			});

			Assert.True(actionExecuted);
			Assert.Same(PubNub.Environment, capturedEnv);
		}
	}
}