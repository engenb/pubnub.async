using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Subscribe;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Services.Subscribe
{
	public class SubscriptionRegistryTests : AbstractTest
	{
		[Fact]
		public void Regiser__Given_EnvironmentChannelAndHandler__When_NewSub__Then_CreateSubAddHandler()
		{
			var handlerInvoked = false;
			MessageReceivedHandler<object> handler = async args =>
			{
				handlerInvoked = true;
				await Task.FromResult(1);
			};

			var subscribeKey = Fixture.Create<string>();
			var authenticationKey = Fixture.Create<string>();

			var channelName = Fixture.Create<string>();

			var channel = new Channel(channelName);

			var message = Fixture
				.Build<PubNubSubscribeResponseMessage>()
				.With(x => x.SubscribeKey, subscribeKey)
				.With(x => x.Channel, channelName)
				.With(x => x.Data, null)
				.Create();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.AuthenticationKey)
				.Returns(authenticationKey);
			mockEnv
				.Setup(x => x.Clone())
				.Returns(mockEnv.Object);

			var expectedSub = new Subscription<object>(mockEnv.Object, channel, Mock.Of<ICryptoService>());

			var mockResolveSub = new Mock<IResolveSubscription>();
			mockResolveSub
				.Setup(x => x.Resolve<object>(mockEnv.Object, channel))
				.Returns(expectedSub);

			var subject = new SubscriptionRegistry(mockResolveSub.Object);

			subject.Register(mockEnv.Object, channel, handler);
			var subs = subject.Get(subscribeKey);
			subject.MessageReceived(message);

			Assert.Contains(expectedSub, subs);
			Assert.True(handlerInvoked);
		}
	}
}
