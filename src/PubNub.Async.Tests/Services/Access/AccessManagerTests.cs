using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Services.Access;
using PubNub.Async.Tests.Common;
using PubNub.Async.Tests.Common.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.Access
{
	public class AccessManagerTests : AbstractTest
	{
		[Fact]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_PreviouslyRegistered__Then_ReturnCachedResponse()
		{
			var expected = Fixture.Create<GrantResponse>();

			var access = AccessType.ReadWrite;
			var channel = new Channel("channel");
			var secKey = Fixture.Create<string>();
			var authKey = Fixture.Create<string>();

			var client = channel
				.ConfigurePubNub(x =>
				{
					x.SecretKey = secKey;
					x.AuthenticationKey = authKey;
				});

			var mockRegistry = new Mock<IAccessRegistry>();
			mockRegistry
				.Setup(x => x.Granted(channel, authKey, access))
				.Returns(true);
			mockRegistry
				.Setup(x => x.CachedRegistration(channel, authKey))
				.ReturnsAsync(expected);

			var subject = new AccessManager(client, mockRegistry.Object);

			var result = await subject.Establish(access);

			Assert.Same(expected, result);
		}

		[Fact]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_Unregistered__Then_GrantAndRegisterResponse()
		{
			var authKey = Fixture.Create<string>();
			var channelName = Fixture.Create<string>();
			var access = AccessType.ReadWrite;
			var pubKey = Fixture.Create<string>();
			var subKey = Fixture.Create<string>();
			var secKey = Fixture.Create<string>();
			var channel = new Channel(channelName);

			var paylaod = Fixture
				.Build<PubNubGrantResponsePayload>()
				.With(x => x.MintuesToExpire, 5)
				.With(x => x.Auths, new Dictionary<string, PubNubGrantResponseAuths>
				{
					{authKey, new PubNubGrantResponseAuths {Read = true, Write = true}}
				})
				.Create();

			var pnResponse = Fixture
				.Build<PubNubGrantResponse>()
				.With(x => x.Status, HttpStatusCode.OK)
				.With(x => x.Paylaod, paylaod)
				.Create();

			var expectedResult = new GrantResponse
			{
				Success = true,
				Message = pnResponse.Message,
				MinutesToExpire = pnResponse.Paylaod.MintuesToExpire,
				Access = access
			};

			var client = channel
				.ConfigurePubNub(x =>
				{
					x.PublishKey = pubKey;
					x.SubscribeKey = subKey;
					x.SecretKey = secKey;
					x.AuthenticationKey = authKey;
				});

			var mockRegistry = new Mock<IAccessRegistry>();
			mockRegistry
				.Setup(x => x.Granted(channel, authKey, access))
				.Returns(false);

			var subject = new AccessManager(client, mockRegistry.Object);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(pnResponse);

				var result = await subject.Establish(access);

				httpTest
					.ShouldHaveCalled($"https://pubsub.pubnub.com/v1/auth/grant/sub-key/{subKey}")
					.WithVerb(HttpMethod.Get)
					.With(c => c.Request.RequestUri.Query.Contains($"channel={channel.Name}"))
					.With(c => c.Request.RequestUri.Query.Contains($"uuid={client.Environment.SessionUuid}"))
					.With(c => c.Request.RequestUri.Query.Contains("timestamp="))
					.With(c => c.Request.RequestUri.Query.Contains($"r={1}"))
					.With(c => c.Request.RequestUri.Query.Contains($"w={1}"))
					.With(c => c.Request.RequestUri.Query.Contains($"auth={authKey}"))
					.With(c => c.Request.RequestUri.Query.Contains("signature="))
					.With(c => !c.Request.RequestUri.Query.Contains("ttl="))
					.Times(1);

				Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));

				mockRegistry.Verify(x => x.Register(channel, authKey, result));
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_UnregisteredNoTTL__Then_GrantRequestAndRegisterResponse()
		{
			var expectedMessage = "Success";
			var expectedMinutesToExpire = 1440; //pn default
			var expectedAccess = AccessType.ReadWrite;

			var authKey = "test-auth";

			var response = await "test-channel"
				.ConfigurePubNub(x =>
				{
					x.PublishKey = Settings.Default.PamPublishKey;
					x.SubscribeKey = Settings.Default.PamSubKey;
					x.SecretKey = Settings.Default.PamSecKey;
					x.AuthenticationKey = authKey;
				})
				.SecuredWith(authKey)
				.Grant(AccessType.ReadWrite);

			Assert.True(response.Success);
			Assert.Equal(expectedMessage, response.Message);
			Assert.Equal(expectedMinutesToExpire, response.MinutesToExpire);
			Assert.Equal(expectedAccess, response.Access);
		}
	}
}