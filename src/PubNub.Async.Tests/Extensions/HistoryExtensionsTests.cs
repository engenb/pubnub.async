using System;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.History;
using PubNub.Async.Services.History;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Extensions
{
	public class HistoryExtensionsTests : AbstractTest, IDisposable
	{
		[Fact]
		public async Task History__Given_StringAndArguments__Then_ResolveInvokeHistory()
		{
			var expectedResult = Fixture.Create<HistoryResponse<object>>();

			var expectedChannelName = Fixture.Create<string>();

			var expectedStart = Fixture.Create<long>();
			var expectedEnd = Fixture.Create<long>();
			var expectedCount = Fixture.Create<int>();
			var expectedOrder = Fixture.Create<HistoryOrder>();
			var expectedIncludeTime = Fixture.Create<bool>();

			IPubNubClient capturedClient = null;

			var mockHistory = new Mock<IHistoryService>();
			mockHistory
				.Setup(x => x.History<object>(expectedStart, expectedEnd, expectedCount, expectedOrder, expectedIncludeTime))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IHistoryService>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockHistory.Object);

			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await expectedChannelName.History<object>(
				expectedStart,
				expectedEnd,
				expectedCount,
				expectedOrder,
				expectedIncludeTime);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		[Fact]
		public async Task History__Given_ChannelAndArguments__Then_ResolveInvokeHistory()
		{
			var expectedResult = Fixture.Create<HistoryResponse<object>>();

			var expectedChannelName = Fixture.Create<string>();
			var channel = new Channel(expectedChannelName);

			var expectedStart = Fixture.Create<long>();
			var expectedEnd = Fixture.Create<long>();
			var expectedCount = Fixture.Create<int>();
			var expectedOrder = Fixture.Create<HistoryOrder>();
			var expectedIncludeTime = Fixture.Create<bool>();

			IPubNubClient capturedClient = null;

			var mockHistory = new Mock<IHistoryService>();
			mockHistory
				.Setup(x => x.History<object>(expectedStart, expectedEnd, expectedCount, expectedOrder, expectedIncludeTime))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IHistoryService>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockHistory.Object);

			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await channel.History<object>(
				expectedStart,
				expectedEnd,
				expectedCount,
				expectedOrder,
				expectedIncludeTime);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		public void Dispose()
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment());
		}
	}
}
