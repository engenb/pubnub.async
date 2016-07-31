using System;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Services.Subscribe;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Services.Subscribe
{
	public class SubscriptionMonitorTests : AbstractTest
	{
		[Fact]
		public void Register__Given_EnvironmentAndSubToken__When_AuthKeyNull__Then_ThrowEx()
		{
			var subject = new SubscriptionMonitor((environment, channel) => null, Mock.Of<ISubscriptionRegistry>());

			Assert.Throws<InvalidOperationException>(() => subject.Register(Mock.Of<IPubNubEnvironment>(), Fixture.Create<long>()));
		}
	}
}
