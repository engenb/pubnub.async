using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Configuration
{
	public class DefaultResolveSubscriptionTests : AbstractTest
	{
		[Fact]
		public void Resolve__Given_EnvironmentAndChannel__Then_ReturnNewSubscription()
		{
			var expectedEnv = Mock.Of<IPubNubEnvironment>();
			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Clone())
				.Returns(expectedEnv);

			var channel = new Channel(Fixture.Create<string>());

			var mockCrypto = new Mock<ICryptoService>();

			var subject = new DefaultResolveSubscription(mockCrypto.Object);

			var result = subject.Resolve<object>(mockEnv.Object, channel);
			
			Assert.NotNull(result);

			Assert.NotSame(channel, result.Channel);
			Assert.Equal(channel, result.Channel);

			Assert.Same(expectedEnv, result.Environment);
		}
	}
}
