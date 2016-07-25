using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Models;
using PubNub.Async.Models.Access;
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
			var access = AccessType.ReadWrite;
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Fixture.Create<string>();

			var response = Fixture
				.Build<GrantResponse>()
				.With(x => x.Access, access)
				.With(x => x.MinutesToExpire, 60)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey, access);

			Assert.True(result);
		}

		[Fact]
		public async Task CachedRegistration__Given_Registered__Then_ReturnPreviousResponse()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Fixture.Create<string>();

			var response = Fixture.Create<GrantResponse>();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = await subject.CachedRegistration(channel, authKey);

			Assert.Equal(JsonConvert.SerializeObject(response), JsonConvert.SerializeObject(result));
		}

		[Fact]
		public async Task Granted__Given_ChannelAuthKeyResponse__When_AccessExpiresInFuture__Then_ReturnTrue()
		{
			var access = AccessType.ReadWrite;
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Fixture.Create<string>();

			var response = Fixture
				.Build<GrantResponse>()
				.With(x => x.Access, access)
				.With(x => x.MinutesToExpire, 60)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey, access);

			Assert.True(result);
		}

		[Fact]
		public async Task Granted__Given_ChannelAuthKeyResponse__When_AccessExpired__Then_ReturnFalse()
		{
			var access = AccessType.ReadWrite;
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Fixture.Create<string>();

			var response = Fixture
				.Build<GrantResponse>()
				.With(x => x.Access, access)
				.With(x => x.MinutesToExpire, -60)
				.Create();

			var subject = new AccessRegistry();
			await subject.Register(channel, authKey, response);

			var result = subject.Granted(channel, authKey, access);

			Assert.False(result);
		}

		[Fact]
		public void Granted__Given_ChannelAuthKey__When_AccessNotRegistered__Then_ReturnFalse()
		{
			var channelName = "channel";
			var channel = new Channel(channelName);

			var authKey = Fixture.Create<string>();

			var subject = new AccessRegistry();

			var result = subject.Granted(channel, authKey, AccessType.ReadWrite);

			Assert.False(result);
		}

		[Fact]
		public async Task Unregister__Given_ChannelAuthKey__When_NotRegistered__Then_DoNothing()
		{
			var channel = new Channel(Fixture.Create<string>());

			var authKey = Fixture.Create<string>();

			var subject = new AccessRegistry();

			subject.Unregister(channel, authKey);

			var registration = await subject.CachedRegistration(channel, authKey);

			Assert.Null(registration);
			// no exceptions were thrown, is the real assert here
		}

		[Fact]
		public void Unregister__Given_NullChannel__Then_ThrowEx()
		{
			var subject = new AccessRegistry();
			var ex = Assert.Throws<ArgumentNullException>(() => subject.Unregister(null, Fixture.Create<string>()));
			Assert.Equal("channel", ex.ParamName);
		}

		[Fact]
		public void Unregister__Given_NullAuthkey__Then_ThrowEx()
		{
			var subject = new AccessRegistry();
			var ex = Assert.Throws<ArgumentNullException>(() => subject.Unregister(new Channel(Fixture.Create<string>()), null));
			Assert.Equal("authenticationKey", ex.ParamName);
		}

		[Fact]
		public void Unregister__Given_EmptyAuthkey__Then_ThrowEx()
		{
			var subject = new AccessRegistry();
			var ex = Assert.Throws<ArgumentNullException>(() => subject.Unregister(new Channel(Fixture.Create<string>()), " "));
			Assert.Equal("authenticationKey", ex.ParamName);
		}

		[Fact]
		public async Task Unregister__Given_ChannelAuthKey__When_Registered__Then_ReturnRegistration()
		{
			var channel = new Channel(Fixture.Create<string>());
			var authKey = Fixture.Create<string>();
			var grant = Fixture.Create<GrantResponse>();

			var subject = new AccessRegistry();

			await subject.Register(channel, authKey, grant);

			var registration = await subject.CachedRegistration(channel, authKey);
			Assert.NotNull(registration);

			subject.Unregister(channel, authKey);
			registration = await subject.CachedRegistration(channel, authKey);
			Assert.Null(registration);
		}
	}
}