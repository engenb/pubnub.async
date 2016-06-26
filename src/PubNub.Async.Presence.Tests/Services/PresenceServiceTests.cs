using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		public async Task SessionState__Given_ConfiguredClientAndState__Then_PublishNewState()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedState = Fixture.Create<PresenceTestState>();

			var expectedStateResponse = new PubNubStateResponse<PresenceTestState>
			{
				Uuid = expectedSessionUuid,
				Status = HttpStatusCode.OK,
				Payload = expectedState,
				Channel = expectedChannel,
				Service = Fixture.Create<string>()
			};

			var expectedPresenceResponse = new SessionStateResponse<PresenceTestState>
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
		public async Task SessionState__Given_ConfiguredClientAndState__Then_GetState()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();
			var expectedSessionUuid = Fixture.Create<string>();
			var expectedState = Fixture.Create<PresenceTestState>();

			var expectedStateResponse = new PubNubStateResponse<PresenceTestState>
			{
				Uuid = expectedSessionUuid,
				Status = HttpStatusCode.OK,
				Payload = expectedState,
				Channel = expectedChannel,
				Service = Fixture.Create<string>()
			};

			var expectedPresenceResponse = new SessionStateResponse<PresenceTestState>
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

			var expectedUrl = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel)
				.AppendPathSegments("uuid", expectedSessionUuid);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(expectedStateResponse);

				var result = await client.SetState(expectedState);

				httpTest.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.Equal(JsonConvert.SerializeObject(expectedPresenceResponse), JsonConvert.SerializeObject(result));
			}
		}

		[Fact]
		public async Task Subscribers__Given_ConfiguredClient__When_ExcludeUuidsAndState__Then_GetOccupancy()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();

			var client = expectedChannel
				.ConfigurePubNub(c =>
				{
					c.SubscribeKey = expectedSubKey;
				});

			var expectedUrl = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel)
				.SetQueryParam("disable_uuids", 1);

			var subscribersResponse = Fixture
				.Build<PubNubSubscriberUuidsResponse>()
				.Without(x => x.Subscribers)
				.Create();

			var expectedResult = new SubscribersResponse<JObject>
			{
				Success = subscribersResponse.Status == HttpStatusCode.OK,
				Message = subscribersResponse.Message,
				Occupancy = subscribersResponse.Occupancy,
				Subscribers = subscribersResponse.Subscribers?
					.Select(x => new Subscriber<JObject> {Uuid = x})
					.ToArray()
			};

			var subject = new PresenceService(client);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(subscribersResponse);

				var result = await subject.Subscribers<JObject>(false, false);

				httpTest.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);

				Assert.NotNull(result);
				Assert.Equal(expectedResult.Success, result.Success);
				Assert.Equal(expectedResult.Message, result.Message);
				Assert.Equal(expectedResult.Occupancy, result.Occupancy);
				Assert.Null(result.Subscribers);
			}
		}

		[Fact]
		public async Task Subscribers__Given_ConfiguredClient__When_IncludeUuidsExcludeState__Then_GetOccupancyAndUuids()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();

			var client = expectedChannel
				.ConfigurePubNub(c =>
				{
					c.SubscribeKey = expectedSubKey;
				});

			var expectedUrl = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel);

			var subscribersResponse = Fixture.Create<PubNubSubscriberUuidsResponse>();

			var expectedResult = new SubscribersResponse<JObject>
			{
				Success = subscribersResponse.Status == HttpStatusCode.OK,
				Message = subscribersResponse.Message,
				Occupancy = subscribersResponse.Occupancy,
				Subscribers = subscribersResponse.Subscribers?
					.Select(x => new Subscriber<JObject> { Uuid = x })
					.ToArray()
			};

			var subject = new PresenceService(client);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(subscribersResponse);

				var result = await subject.Subscribers<JObject>();

				httpTest.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);
				
				Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
			}
		}

		[Fact]
		public async Task Subscribers__Given_ConfiguredClient__When_IncludeUuidsAndState__Then_GetOccupancyUuidsAndStates()
		{
			var expectedSubKey = Fixture.Create<string>();
			var expectedChannel = Fixture.Create<string>();

			var client = expectedChannel
				.ConfigurePubNub(c =>
				{
					c.SubscribeKey = expectedSubKey;
				});

			var expectedUrl = client.Environment.Host
				.AppendPathSegments("v2", "presence")
				.AppendPathSegments("sub_key", expectedSubKey)
				.AppendPathSegments("channel", expectedChannel)
				.SetQueryParam("state", 1);

			var subscribersResponse = Fixture.Create<PubNubSubscribersResponse<JObject>>();

			var expectedResult = new SubscribersResponse<JObject>
			{
				Success = subscribersResponse.Status == HttpStatusCode.OK,
				Message = subscribersResponse.Message,
				Occupancy = subscribersResponse.Occupancy,
				Subscribers = subscribersResponse.Subscribers
			};

			var subject = new PresenceService(client);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(subscribersResponse);

				var result = await subject.Subscribers<JObject>(true);

				httpTest.ShouldHaveCalled(expectedUrl)
					.WithVerb(HttpMethod.Get)
					.Times(1);
				
				Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(result));
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task SessionState__Given_ConfiguredClientAndState__Then_StateUpdated()
		{
			var state = Fixture.Create<PresenceTestState>();

			var client = Settings.Default.PresenceTestChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = "presence-test-session";
					c.SubscribeKey = Settings.Default.SubscribeKey;
				});

			var subject = new PresenceService(client);

			var setResult = await subject.SessionState(state);

			Assert.NotNull(setResult);
			Assert.NotSame(state, setResult);
			Assert.True(setResult.Success);
			Assert.Equal(state.Foo, setResult.State.Foo);
			Assert.Equal(state.Bar, setResult.State.Bar);
			Assert.Equal(state.Fubar, setResult.State.Fubar);

			var getResult = await subject.SessionState<PresenceTestState>();

			Assert.NotNull(getResult);
			Assert.NotSame(state, getResult);
			Assert.True(getResult.Success);
			Assert.Equal(state.Foo, getResult.State.Foo);
			Assert.Equal(state.Bar, getResult.State.Bar);
			Assert.Equal(state.Fubar, getResult.State.Fubar);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Subscribers__Given_ConfiguratedClient__When_ExcludeUuidsAndState__Then_FetchOccupancy()
		{
			var client = Settings.Default.PresenceTestChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = "presence-test-session";
					c.SubscribeKey = Settings.Default.SubscribeKey;
				});

			var subject = new PresenceService(client);

			var result = await subject.Subscribers<JObject>(false, false);

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.Null(result.Subscribers);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Subscribers__Given_ConfiguratedClient__When_IncludeUuidsExcludeState__Then_FetchOccupancyAndUuids()
		{
			var client = Settings.Default.PresenceTestChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = "presence-test-session";
					c.SubscribeKey = Settings.Default.SubscribeKey;
				});

			var subject = new PresenceService(client);

			var result = await subject.Subscribers<JObject>();

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.NotNull(result.Subscribers);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Subscribers__Given_ConfiguratedClient__When_IncludeUuidsAndState__Then_FetchOccupancyUuidsAndState()
		{
			var client = Settings.Default.PresenceTestChannel
				.ConfigurePubNub(c =>
				{
					c.SessionUuid = "presence-test-session";
					c.SubscribeKey = Settings.Default.SubscribeKey;
				});

			var subject = new PresenceService(client);

			var result = await subject.Subscribers<JObject>(true);

			Assert.NotNull(result);
			Assert.True(result.Success);
			Assert.NotNull(result.Subscribers);
		}

		public class PresenceTestState
		{
			public string Foo { get; set; }
			public long Bar { get; set; }
			public string[] Fubar { get; set; }
		}
	}
}