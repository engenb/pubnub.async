using System;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Publish;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Extensions
{
	public class PublishExtensionsTests : AbstractTest, IDisposable
	{
		[Fact]
		public async Task Publish__Given_StringAndMessage__Then_Publish()
		{
			var expectedResult = Fixture.Create<PublishResponse>();

			var expectedChannelName = Fixture.Create<string>();
			var expectedMessage = new object();

			IPubNubClient capturedClient = null;

			var mockPub = new Mock<IPublishService>();
			mockPub
				.Setup(x => x.Publish(expectedMessage, true))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IPublishService>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockPub.Object);

			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await expectedChannelName.Publish(expectedMessage);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		[Fact]
		public async Task Publish__Given_ChannelAndMessage__Then_Publish()
		{
			var expectedResult = Fixture.Create<PublishResponse>();

			var expectedChannelName = Fixture.Create<string>();
			var expectedMessage = new object();

			var channel = new Channel(expectedChannelName);

			IPubNubClient capturedClient = null;

			var mockPub = new Mock<IPublishService>();
			mockPub
				.Setup(x => x.Publish(expectedMessage, true))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IPublishService>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockPub.Object);

			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await channel.Publish(expectedMessage);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		public void Dispose()
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment());
		}
	}
}
