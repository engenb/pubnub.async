using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Configuration;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Channel;
using PubNub.Async.Services.Access;
using PubNub.Async.Tests.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.Access
{
	public class AccessManagerTests : AbstractTest
	{
		[Fact]
		public async Task Establish__Given_Channel__When_ClientSecretKeyNotConfigured__Then_ThrowEx()
		{
			var subject = new AccessManager(new PubNubClient("channel"), Mock.Of<IAccessRegistry>());

			await Assert.ThrowsAsync<InvalidOperationException>(() => subject.Establish(AccessType.ReadWrite));
		}

		[Fact]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_PreviouslyRegistered__Then_ReturnCachedResponse()
		{
			var expected = Fixture.Create<AccessGrantResponse>();

			var channel = new Channel("channel");
			var secKey = Guid.NewGuid().ToString();
			var authKey = Guid.NewGuid().ToString();

			var client = channel
				.ConfigurePubNub(x =>
				{
					x.SecretKey = secKey;
					x.AuthenticationKey = authKey;
				});

			var mockRegistry = new Mock<IAccessRegistry>();
			mockRegistry
				.Setup(x => x.InForce(channel, authKey))
				.Returns(true);
			mockRegistry
				.Setup(x => x.Registration(channel, authKey))
				.ReturnsAsync(expected);

			var subject = new AccessManager(client, mockRegistry.Object);

			var result = await subject.Establish(AccessType.ReadWrite);

			Assert.Same(expected, result);
		}

		[Fact]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_Unregistered__Then_GrantAndRegisterResponse()
		{
			var paylaod = Fixture
				.Build<AccessGrantResponsePayload>()
				.With(x => x.MintuesToExpire, 5)
				.Create();

			var expected = Fixture
				.Build<AccessGrantResponse>()
				.With(x => x.Paylaod, paylaod)
				.Create();

			var pubKey = Guid.NewGuid().ToString();
			var subKey = Guid.NewGuid().ToString();
			var secKey = Guid.NewGuid().ToString();
			var channel = new Channel("test-channel");
			var authKey = Guid.NewGuid().ToString();

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
				.Setup(x => x.InForce(channel, authKey))
				.Returns(false);
			
			var subject = new AccessManager(client, mockRegistry.Object);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expected);

				var result = await subject.Establish(AccessType.ReadWrite);

				httpTest
					.ShouldHaveCalled($"https://pubsub.pubnub.com/v1/auth/grant/sub-key/{subKey}")
					.WithVerb(HttpMethod.Get)
					.With(c => c.Request.RequestUri.Query.Contains($"channel={channel.Name}"))
					.With(c => c.Request.RequestUri.Query.Contains($"uuid={client.Settings.SessionUuid}"))
					.With(c => c.Request.RequestUri.Query.Contains("timestamp="))
					.With(c => c.Request.RequestUri.Query.Contains($"r={1}"))
					.With(c => c.Request.RequestUri.Query.Contains($"w={1}"))
					.With(c => c.Request.RequestUri.Query.Contains($"auth={authKey}"))
					.With(c => c.Request.RequestUri.Query.Contains("signature="))
					.With(c => !c.Request.RequestUri.Query.Contains("ttl="))
					.Times(1);
				
				Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(result));

				mockRegistry.Verify(x => x.Register(channel, authKey, result));
			}
		}

		[Fact(Skip = "L&L")]
		[Trait("Category", "integration")]
		public async Task Establish__Given_ChannelAndConfiguredClient__When_Unregistered__Then_GrantRequestAndRegisterResponse()
		{
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

			Assert.Equal(HttpStatusCode.OK, response.Status);
		}
	}
}
