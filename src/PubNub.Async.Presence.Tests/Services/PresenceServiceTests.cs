using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http.Testing;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using PubNub.Async.Extensions;
using PubNub.Async.Presence.Configuration;
using PubNub.Async.Presence.Extensions;
using PubNub.Async.Presence.Models;
using PubNub.Async.Presence.Services;
using PubNub.Async.Tests.Common;
using PubNub.Async.Tests.Common.Properties;
using Xunit;

namespace PubNub.Async.Presence.Tests.Services
{
	public class PresenceServiceTests : AbstractTest
	{
		public PresenceServiceTests()
		{
			PubNub.Environment.UsePresence();
		}

		[Fact]
		public async Task SetState__Given_ConfiguredClientAndState__Then_PublishNewState()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedState = Fixture.Create<PresenceTestState>();

			var expectedStateResponse = new StateResponse<PresenceTestState>
			{
				Uuid = expectedSessionUuid,
				Status = HttpStatusCode.OK,
				Payload = expectedState,
				Channel = expectedChannel,
				Service = Fixture.Create<string>()
			};

			var expectedPresenceResponse = new PresenceResponse<PresenceTestState>
			{
				Success = true,
				State = expectedState
			};

			var client = expectedChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = expectedSessionUuid;
					c.SubscribeKey = expectedSubKey;
				});

			var expectedUri = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel)
				.AppendPathSegments("uuid", expectedSessionUuid)
				.AppendPathSegment("data")
				.SetQueryParam("state", JsonConvert.SerializeObject(expectedState));

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedStateResponse);

				var result = await client.SetState(expectedState);

				httpTest.ShouldHaveCalled(expectedUri)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.Equal(JsonConvert.SerializeObject(expectedPresenceResponse), JsonConvert.SerializeObject(result));
			}
		}

		[Fact]
		public async Task GetState__Given_ConfiguredClientAndState__Then_GetState()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedState = Fixture.Create<PresenceTestState>();

			var expectedStateResponse = new StateResponse<PresenceTestState>
			{
				Uuid = expectedSessionUuid,
				Status = HttpStatusCode.OK,
				Payload = expectedState,
				Channel = expectedChannel,
				Service = Fixture.Create<string>()
			};

			var expectedPresenceResponse = new PresenceResponse<PresenceTestState>
			{
				Success = true,
				State = expectedState
			};

			var client = expectedChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = expectedSessionUuid;
					c.SubscribeKey = expectedSubKey;
				});

			var expectedUri = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel)
				.AppendPathSegments("uuid", expectedSessionUuid);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedStateResponse);

				var result = await client.SetState(expectedState);

				httpTest.ShouldHaveCalled(expectedUri)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.Equal(JsonConvert.SerializeObject(expectedPresenceResponse), JsonConvert.SerializeObject(result));
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task SetState__Given_ConfiguredClientAndState__Then_StateUpdated()
		{
			var state = Fixture.Create<PresenceTestState>();

			var client = Settings.Default.PresenceTestChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = "presence-test-session";
					c.SubscribeKey = Settings.Default.SubscribeKey;
				});

			var subject = new PresenceService(client);

			await subject.SetState(state);

			var result = await subject.GetState<PresenceTestState>();

			Assert.NotSame(state, result);
			Assert.True(result.Success);
			Assert.Equal(state.Foo, result.State.Foo);
			Assert.Equal(state.Bar, result.State.Bar);
			Assert.Equal(state.Fubar, result.State.Fubar);
		}

		public class PresenceTestState
		{
			public string Foo { get; set; }
			public long Bar { get; set; }
			public string[] Fubar { get; set; }
		}
	}
}