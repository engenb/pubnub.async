using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Services.Access;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Services.Access
{
	public class AccessRegistryTests : AbstractTest
	{
		[Fact]
		public async Task Register__Given_ChannelAuthKeyResponse__When_AccessExpiresInFuture__Then_UpdateRegistry()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Guid.NewGuid().ToString();

			var payload = Fixture
				.Build<AccessGrantResponsePayload>()
				.With(x => x.MintuesToExpire, 60)
				.Create();

			var response = Fixture
				.Build<AccessGrantResponse>()
				.With(x => x.Paylaod, payload)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey);

			Assert.True(result);
		}

		[Fact]
		public async Task Registration__Given_Registered__Then_ReturnPreviousResponse()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Guid.NewGuid().ToString();

			var response = Fixture.Create<AccessGrantResponse>();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = await subject.Registration(channel, authKey);

			Assert.Equal(JsonConvert.SerializeObject(response), JsonConvert.SerializeObject(result));
		}

		[Fact]
		public async Task InForce__Given_ChannelAuthKeyResponse__When_AccessExpiresInFuture__Then_ReturnTrue()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Guid.NewGuid().ToString();

			var payload = Fixture
				.Build<AccessGrantResponsePayload>()
				.With(x => x.MintuesToExpire, 60)
				.Create();

			var response = Fixture
				.Build<AccessGrantResponse>()
				.With(x => x.Paylaod, payload)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey);

			Assert.True(result);
		}

		[Fact]
		public async Task InForce__Given_ChannelAuthKeyResponse__When_AccessExpired__Then_ReturnFalse()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Guid.NewGuid().ToString();

			var payload = Fixture
				.Build<AccessGrantResponsePayload>()
				.With(x => x.MintuesToExpire, -60)
				.Create();

			var response = Fixture
				.Build<AccessGrantResponse>()
				.With(x => x.Paylaod, payload)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey);

			Assert.False(result);
		}

		[Fact]
		public void InForce__Given_ChannelAuthKey__When_AccessNotRegistered__ThenReturnFalse()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Guid.NewGuid().ToString();

			var subject = new AccessRegistry();

			var result = subject.Granted(channel, authKey);

			Assert.False(result);
		}
	}
}
