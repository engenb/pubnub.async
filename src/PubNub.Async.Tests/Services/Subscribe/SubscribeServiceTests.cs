using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http.Testing;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Subscribe;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Subscribe;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Services.Subscribe
{
	public class SubscribeServiceTests : AbstractTest
	{
		[Fact]
		public async Task Subscribe__Given_EnvironmentAndChannel__When_UnsecuredChannelWithoutPAM__Then_RegisterSubscription()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var host = "https://pubsub.pubnub.com";
			var subscribeKey = Guid.NewGuid().ToString();
			var sessionUuid = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName);

			var subscriptions = new List<Subscription>();

			var expectedTime = Fixture.Create<long>();
			var expectedResponse = Fixture
				.Build<PubNubSubscribeResponse>()
				.With(x => x.SubscribeTime, Fixture
					.Build<PubNubSubscribeResponseTime>()
					.With(x => x.TimeToken, expectedTime)
					.Create())
				.With(x => x.Messages, Fixture
					.Build<PubNubSubscribeResponseMessage>()
					.With(x => x.Data, null)
					.CreateMany()
					.ToArray())
				.Create();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns(host);
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var mockMonitor = new Mock<ISubscriptionMonitor>();
			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(() => subscriptions.ToArray());
			mockRegistry
				.Setup(x => x.Register(mockEnv.Object, channel, handler))
				.Callback<IPubNubEnvironment, Channel, MessageReceivedHandler<object>>((e, c, h) =>
					subscriptions.Add(new Subscription<object>(e, c, Mock.Of<ICryptoService>())));

			var subject = new SubscribeService(
				mockClient.Object,
				Mock.Of<IAccessManager>(),
				mockMonitor.Object,
				mockRegistry.Object);

			var expectedUrl = host
				.AppendPathSegments("v2", "subscribe")
				.AppendPathSegment(subscribeKey)
				.AppendPathSegment(channel.Name)
				.AppendPathSegment("0")
				.SetQueryParam("uuid", sessionUuid);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedResponse);

				var result = await subject.Subscribe(handler);

				httpTest
					.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.NotNull(result);
				Assert.True(result.Success);
				Assert.Equal(expectedTime, result.SubscribeTime);
			}

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Register(mockEnv.Object, channel, handler), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Once);
		}

		[Fact]
		public async Task Subscribe__Given_EnvironmentAndChannel__When_UnsecuredChannelWithPAM__Then_Handle403DontRegister()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var host = "https://pubsub.pubnub.com";
			var subscribeKey = Guid.NewGuid().ToString();
			var sessionUuid = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName);

			var subscriptions = new List<Subscription>();

			var expectedResponseMessage = Fixture.Create<string>();
			var expectedResponse = Fixture
				.Build<PubNubForbiddenResponse>()
				.With(x => x.Message, expectedResponseMessage)
				.Create();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns(host);
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var mockMonitor = new Mock<ISubscriptionMonitor>();
			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(() => subscriptions.ToArray());
			mockRegistry
				.Setup(x => x.Register(mockEnv.Object, channel, handler))
				.Callback<IPubNubEnvironment, Channel, MessageReceivedHandler<object>>((e, c, h) =>
					subscriptions.Add(new Subscription<object>(e, c, Mock.Of<ICryptoService>())));

			var subject = new SubscribeService(
				mockClient.Object,
				Mock.Of<IAccessManager>(),
				mockMonitor.Object,
				mockRegistry.Object);

			var expectedUrl = host
				.AppendPathSegments("v2", "subscribe")
				.AppendPathSegment(subscribeKey)
				.AppendPathSegment(channel.Name)
				.AppendPathSegment("0")
				.SetQueryParam("uuid", sessionUuid);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedResponse, (int)HttpStatusCode.Forbidden);

				var result = await subject.Subscribe(handler);

				httpTest
					.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.NotNull(result);
				Assert.False(result.Success);
				Assert.Equal(expectedResponseMessage, result.Message);
			}

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Register(mockEnv.Object, channel, handler), Times.Never);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Never);
		}

		[Fact]
		public async Task Subscribe__Given_EnvironmentAndChannel__When_SecuredChannelAndEnvironmentGrantCapable__Then_EstablishAccessAndRegisterSubscription()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var host = "https://pubsub.pubnub.com";
			var subscribeKey = Guid.NewGuid().ToString();
			var sessionUuid = Guid.NewGuid().ToString();
			var authenticationKey = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName)
			{
				Secured = true
			};

			var subscriptions = new List<Subscription>();

			var expectedTime = Fixture.Create<long>();
			var expectedResponse = Fixture
				.Build<PubNubSubscribeResponse>()
				.With(x => x.SubscribeTime, Fixture
					.Build<PubNubSubscribeResponseTime>()
					.With(x => x.TimeToken, expectedTime)
					.Create())
				.With(x => x.Messages, Fixture
					.Build<PubNubSubscribeResponseMessage>()
					.With(x => x.Data, null)
					.CreateMany()
					.ToArray())
				.Create();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns(host);
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);
			mockEnv
				.SetupGet(x => x.AuthenticationKey)
				.Returns(authenticationKey);
			mockEnv
				.Setup(x => x.GrantCapable())
				.Returns(true);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var expectedGrantResponse = Fixture
				.Build<GrantResponse>()
				.With(x => x.Success, true)
				.Create();

			var mockAccess = new Mock<IAccessManager>();
			mockAccess
				.Setup(x => x.Establish(AccessType.Read))
				.ReturnsAsync(expectedGrantResponse);

			var mockMonitor = new Mock<ISubscriptionMonitor>();

			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(() => subscriptions.ToArray());
			mockRegistry
				.Setup(x => x.Register(mockEnv.Object, channel, handler))
				.Callback<IPubNubEnvironment, Channel, MessageReceivedHandler<object>>((e, c, h) =>
					subscriptions.Add(new Subscription<object>(e, c, Mock.Of<ICryptoService>())));

			var subject = new SubscribeService(
				mockClient.Object,
				mockAccess.Object,
				mockMonitor.Object,
				mockRegistry.Object);

			var expectedUrl = host
				.AppendPathSegments("v2", "subscribe")
				.AppendPathSegment(subscribeKey)
				.AppendPathSegment(channel.Name)
				.AppendPathSegment("0")
				.SetQueryParam("uuid", sessionUuid)
				.SetQueryParam("auth", authenticationKey);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedResponse);

				var result = await subject.Subscribe(handler);

				httpTest
					.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.NotNull(result);
				Assert.True(result.Success);
				Assert.Equal(expectedTime, result.SubscribeTime);
			}

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Register(mockEnv.Object, channel, handler), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Once);
		}

		[Fact]
		public async Task Subscribe__Given_EnvironmentAndChannel__When_SecuredChannelAndNoEnvironmentGrantCapable__Then_RegisterSubscription()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var host = "https://pubsub.pubnub.com";
			var subscribeKey = Guid.NewGuid().ToString();
			var sessionUuid = Guid.NewGuid().ToString();
			var authenticationKey = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName)
			{
				Secured = true
			};

			var subscriptions = new List<Subscription>();

			var expectedTime = Fixture.Create<long>();
			var expectedResponse = Fixture
				.Build<PubNubSubscribeResponse>()
				.With(x => x.SubscribeTime, Fixture
					.Build<PubNubSubscribeResponseTime>()
					.With(x => x.TimeToken, expectedTime)
					.Create())
				.With(x => x.Messages, Fixture
					.Build<PubNubSubscribeResponseMessage>()
					.With(x => x.Data, null)
					.CreateMany()
					.ToArray())
				.Create();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns(host);
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);
			mockEnv
				.SetupGet(x => x.AuthenticationKey)
				.Returns(authenticationKey);
			mockEnv
				.Setup(x => x.GrantCapable())
				.Returns(false);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var mockAccess = new Mock<IAccessManager>();

			var mockMonitor = new Mock<ISubscriptionMonitor>();

			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(() => subscriptions.ToArray());
			mockRegistry
				.Setup(x => x.Register(mockEnv.Object, channel, handler))
				.Callback<IPubNubEnvironment, Channel, MessageReceivedHandler<object>>((e, c, h) =>
					subscriptions.Add(new Subscription<object>(e, c, Mock.Of<ICryptoService>())));

			var subject = new SubscribeService(
				mockClient.Object,
				mockAccess.Object,
				mockMonitor.Object,
				mockRegistry.Object);

			var expectedUrl = host
				.AppendPathSegments("v2", "subscribe")
				.AppendPathSegment(subscribeKey)
				.AppendPathSegment(channel.Name)
				.AppendPathSegment("0")
				.SetQueryParam("uuid", sessionUuid)
				.SetQueryParam("auth", authenticationKey);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedResponse);

				var result = await subject.Subscribe(handler);

				httpTest
					.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.NotNull(result);
				Assert.True(result.Success);
				Assert.Equal(expectedTime, result.SubscribeTime);
			}

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Register(mockEnv.Object, channel, handler), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Once);

			mockAccess.Verify(x => x.Establish(It.IsAny<AccessType>()), Times.Never);
		}

		[Fact]
		public async Task Subscribe__Given_EnvironmentAndChannel__When_SecuredChannelEnvironmentGrantCapableButGrantFails__Then_DontRegister()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var host = "https://pubsub.pubnub.com";
			var subscribeKey = Guid.NewGuid().ToString();
			var sessionUuid = Guid.NewGuid().ToString();
			var authenticationKey = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName)
			{
				Secured = true
			};

			var subscriptions = new List<Subscription>();

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns(host);
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);
			mockEnv
				.SetupGet(x => x.AuthenticationKey)
				.Returns(authenticationKey);
			mockEnv
				.Setup(x => x.GrantCapable())
				.Returns(true);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var expectedGrantResponse = Fixture
				.Build<GrantResponse>()
				.With(x => x.Success, false)
				.Create();

			var mockAccess = new Mock<IAccessManager>();
			mockAccess
				.Setup(x => x.Establish(AccessType.Read))
				.ReturnsAsync(expectedGrantResponse);

			var mockMonitor = new Mock<ISubscriptionMonitor>();

			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(() => subscriptions.ToArray());
			mockRegistry
				.Setup(x => x.Register(mockEnv.Object, channel, handler))
				.Callback<IPubNubEnvironment, Channel, MessageReceivedHandler<object>>((e, c, h) =>
					subscriptions.Add(new Subscription<object>(e, c, Mock.Of<ICryptoService>())));

			var subject = new SubscribeService(
				mockClient.Object,
				mockAccess.Object,
				mockMonitor.Object,
				mockRegistry.Object);

			using (var httpTest = new HttpTest())
			{
				var result = await subject.Subscribe(handler);

				httpTest.ShouldNotHaveCalled("*");

				Assert.NotNull(result);
				Assert.False(result.Success);
				Assert.Equal(expectedGrantResponse.Message, result.Message);
			}

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Register(mockEnv.Object, channel, handler), Times.Never);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Never);
		}

		[Fact]
		public async Task Unsubscribe__Given_EnvironmentAndChannel__Then_RemoveChannelSubscription()
		{
			var subscribeKey = Fixture.Create<string>();
			var sessionUuid = Fixture.Create<string>();
			var channelName = Fixture.Create<string>();

			long? stopCalledTicks = null;
			long? startCalledTicks = null;
			long? unregisterCalledTicks = null;

			var channel = new Channel(channelName);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.Host)
				.Returns("https://pubsub.pubnub.com");
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);
			mockEnv
				.SetupGet(x => x.SessionUuid)
				.Returns(sessionUuid);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);

			var mockAccess = new Mock<IAccessManager>();

			var mockMonitor = new Mock<ISubscriptionMonitor>();
			mockMonitor
				.Setup(x => x.Stop(mockEnv.Object))
				.Callback(() => stopCalledTicks = DateTime.UtcNow.Ticks)
				.Returns(Task.FromResult(1));
			mockMonitor
				.Setup(x => x.Start(mockEnv.Object))
				.Callback(() => startCalledTicks = DateTime.UtcNow.Ticks)
				.Returns(Task.FromResult(1));

			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Unregister(mockEnv.Object, channel))
				.Callback(() => unregisterCalledTicks = DateTime.UtcNow.Ticks);
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(new Subscription[] { new Subscription<string>(mockEnv.Object, channel, Mock.Of<ICryptoService>()) });

			var subject = new SubscribeService(
				mockClient.Object,
				mockAccess.Object,
				mockMonitor.Object,
				mockRegistry.Object);

			await subject.Unsubscribe();

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockRegistry.Verify(x => x.Unregister(mockEnv.Object, channel), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Once);

			Assert.True(stopCalledTicks <= unregisterCalledTicks);
			Assert.True(stopCalledTicks <= startCalledTicks);
			Assert.True(unregisterCalledTicks <= startCalledTicks);
		}

		[Fact]
		public async Task Unsubscribe__Given_EnvironmentChannelAndHandler__When_RemainingSubs__Then_RemoveHandlerAndStartMonitor()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var subscribeKey = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var mockMonitor = new Mock<ISubscriptionMonitor>();
			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(new Subscription[] { new Subscription<object>(mockEnv.Object, channel, Mock.Of<ICryptoService>()) });

			var subject = new SubscribeService(
				mockClient.Object,
				Mock.Of<IAccessManager>(),
				mockMonitor.Object,
				mockRegistry.Object);

			await subject.Unsubscribe(handler);

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Once);

			mockRegistry.Verify(x => x.Unregister(mockEnv.Object, channel, handler), Times.Once);
		}

		[Fact]
		public async Task Unsubscribe__Given_EnvironmentChannelAndHandler__When_NoRemainingSubs__Then_RemoveHandler()
		{
			MessageReceivedHandler<object> handler = async args => { await Task.FromResult(1); };

			var subscribeKey = Guid.NewGuid().ToString();
			var channelName = Guid.NewGuid().ToString();

			var channel = new Channel(channelName);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.SetupGet(x => x.SubscribeKey)
				.Returns(subscribeKey);

			var mockClient = new Mock<IPubNubClient>();
			mockClient
				.SetupGet(x => x.Environment)
				.Returns(mockEnv.Object);
			mockClient
				.SetupGet(x => x.Channel)
				.Returns(channel);

			var mockMonitor = new Mock<ISubscriptionMonitor>();
			var mockRegistry = new Mock<ISubscriptionRegistry>();
			mockRegistry
				.Setup(x => x.Get(subscribeKey))
				.Returns(new Subscription[0]);

			var subject = new SubscribeService(
				mockClient.Object,
				Mock.Of<IAccessManager>(),
				mockMonitor.Object,
				mockRegistry.Object);

			await subject.Unsubscribe(handler);

			mockMonitor.Verify(x => x.Stop(mockEnv.Object), Times.Once);
			mockMonitor.Verify(x => x.Start(mockEnv.Object), Times.Never);

			mockRegistry.Verify(x => x.Unregister(mockEnv.Object, channel, handler), Times.Once);
		}
	}
}
