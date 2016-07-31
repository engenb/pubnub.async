using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
using PubNub.Async.Services.Access;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Extensions
{
	public class AccessExtensionsTests : AbstractTest, IDisposable
	{
		[Fact]
		public async Task Grant__Given_StringAndAccessType__Then_EstablishAccess()
		{
			var expectedResult = Fixture.Create<GrantResponse>();

			var expectedAccessType = Fixture.Create<AccessType>();
			var expectedChannelName = Fixture.Create<string>();

			IPubNubClient capturedClient = null;

			var mockAccess = new Mock<IAccessManager>();
			mockAccess
				.Setup(x => x.Establish(expectedAccessType))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IAccessManager>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockAccess.Object);
			
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await expectedChannelName.Grant(expectedAccessType);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		[Fact]
		public async Task Grant__Given_ChannelAndAccessType__Then_EstablishAccess()
		{
			var expectedResult = Fixture.Create<GrantResponse>();

			var expectedAccessType = Fixture.Create<AccessType>();
			var expectedChannelName = Fixture.Create<string>();

			var channel = new Channel(expectedChannelName);

			IPubNubClient capturedClient = null;

			var mockAccess = new Mock<IAccessManager>();
			mockAccess
				.Setup(x => x.Establish(expectedAccessType))
				.ReturnsAsync(expectedResult);

			var mockEnv = new Mock<IPubNubEnvironment>();
			mockEnv
				.Setup(x => x.Resolve<IAccessManager>(It.IsAny<IPubNubClient>()))
				.Callback<IPubNubClient>(x => capturedClient = x)
				.Returns(mockAccess.Object);

			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => mockEnv.Object);

			var result = await channel.Grant(expectedAccessType);

			Assert.Equal(expectedChannelName, capturedClient.Channel.Name);
			Assert.Same(expectedResult, result);
		}

		public void Dispose()
		{
			PubNub.InternalEnvironment = new Lazy<IPubNubEnvironment>(() => new DefaultPubNubEnvironment());
		}
	}
}
