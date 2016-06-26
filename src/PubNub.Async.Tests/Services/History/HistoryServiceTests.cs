using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using PubNub.Async.Extensions;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.History;
using PubNub.Async.Tests.Common;
using PubNub.Async.Tests.Common.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.History
{
	public class HistoryServiceTests : AbstractTest
	{
		[Fact]
		public async Task History__Given_ConfiguredPubNub__When_SSLOn__Then_Https()
		{
			var client = "channel"
				.ConfigurePubNub(c =>
				{
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
					c.SslEnabled = true;
				});

			var mockCrypto = new Mock<ICryptoService>();
			var mockAccess = new Mock<IAccessManager>();

			var subject = new HistoryService(client, mockCrypto.Object, mockAccess.Object);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] {new HistoryTestMessage[] {}, 1234, 1234});

				await subject.History<HistoryTestMessage>();

				httpTest.ShouldHaveCalled("https://*")
					.WithVerb(HttpMethod.Get)
					.Times(1);
			}
		}

		[Fact]
		public async Task History__Given_ConfiguredPubNub__When_SSLOff__Then_Http()
		{
			var client = "channel"
				.ConfigurePubNub(c =>
				{
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
					c.SslEnabled = false;
				});

			var mockCrypto = new Mock<ICryptoService>();
			var mockAccess = new Mock<IAccessManager>();

			var subject = new HistoryService(client, mockCrypto.Object, mockAccess.Object);

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] {new HistoryTestMessage[] {}, 1234, 1234});

				await subject.History<HistoryTestMessage>();

				httpTest.ShouldHaveCalled("http://*")
					.WithVerb(HttpMethod.Get)
					.Times(1);
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNubWithSSL__When_HistoryNotEnabled__Then_GetError()
		{
			var expectedError =
				"Use of the history API requires the Storage & Playback add-on which is not enabled for this subscribe key. " +
				"Login to your PubNub Dashboard Account and ADD the Storage & Playback add-on. " +
				"Contact support@pubnub.com if you require further assistance.";

			var response = await Settings.Default.HistoryDecryptedChannel
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey)
				.History<HistoryTestMessage>(count: 3, order: HistoryOrder.Reverse);

			Assert.Equal(0, response.Oldest);
			Assert.Equal(0, response.Oldest);
			Assert.Null(response.Messages);
			Assert.Equal(expectedError, response.Error);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNubWithSSL__When_UnencryptedCountIsThreeAndReverse__Then_GetFirstThree()
		{
			var expectedCount = 3;

			var response = await Settings.Default.HistoryDecryptedChannel
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.SubscribeKey)
				.History<HistoryTestMessage>(count: expectedCount, order: HistoryOrder.Reverse);

			Assert.NotNull(response.Messages);
			Assert.Equal(expectedCount, response.Messages.Count());

			var messages = response.Messages.ToArray();
			Assert.Equal("three", messages[0].Content.Message);
			Assert.Equal(14621647091558573L, messages[0].Sent);
			Assert.Equal("two", messages[1].Content.Message);
			Assert.Equal(14621647057951202L, messages[1].Sent);
			Assert.Equal("one", messages[2].Content.Message);
			Assert.Equal(14621647024027759L, messages[2].Sent);

			Assert.Equal(14621647024027759L, response.Oldest);
			Assert.Equal(14621647091558573L, response.Newest);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNubWithSSL__When_TimeOmitted__Then_GetFirstThreeWithoutTime()
		{
			var expectedCount = 3;

			var response = await Settings.Default.HistoryDecryptedChannel
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.SubscribeKey)
				.History<HistoryTestMessage>(count: expectedCount, order: HistoryOrder.Reverse, includeTime: false);

			Assert.NotNull(response.Messages);
			Assert.Equal(expectedCount, response.Messages.Count());

			var messages = response.Messages.ToArray();
			Assert.Equal("three", messages[0].Content.Message);
			Assert.Null(messages[0].Sent);
			Assert.Equal("two", messages[1].Content.Message);
			Assert.Null(messages[1].Sent);
			Assert.Equal("one", messages[2].Content.Message);
			Assert.Null(messages[2].Sent);

			Assert.Equal(14621647024027759L, response.Oldest);
			Assert.Equal(14621647091558573L, response.Newest);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNubWithSSL__When_EncryptedCountIsThreeAndReverse__Then_GetDecryptFirstThree()
		{
			var expectedCount = 3;

			var response = await Settings.Default.HistoryEncryptedChannel
				.EncryptedWith(Settings.Default.CipherKey)
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.SubscribeKey)
				.History<HistoryTestMessage>(count: expectedCount, order: HistoryOrder.Reverse);

			Assert.NotNull(response?.Messages);
			Assert.Equal(expectedCount, response.Messages.Length);

			var messages = response.Messages.ToArray();
			Assert.Equal("three", messages[0].Content.Message);
			Assert.Equal(14646739500961712L, messages[0].Sent);
			Assert.Equal("two", messages[1].Content.Message);
			Assert.Equal(14646739476339247L, messages[1].Sent);
			Assert.Equal("one", messages[2].Content.Message);
			Assert.Equal(14646739446049504L, messages[2].Sent);

			Assert.Equal(14646739446049504L, response.Oldest);
			Assert.Equal(14646739500961712L, response.Newest);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNub__When_Count250Reverse__Then_Fetch250InReverseOrder()
		{
			var expectedCount = 250;
			var expectedFirst = 14651593893212854;
			var expectedLast = 14651598898699987;

			var response = await Settings.Default.HistoryEncryptedHighVolumeChannel
				.EncryptedWith(Settings.Default.CipherKey)
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.SubscribeKey)
				.History<HistoryTestMessage>(count: expectedCount, order: HistoryOrder.Reverse);

			Assert.NotNull(response.Messages);
			Assert.Equal(expectedCount, response.Messages.Length);

			Assert.Equal(expectedFirst, response.Oldest);
			Assert.Equal(expectedLast, response.Newest);

			var messages = response.Messages.ToArray();
			//assert messages are in proper order
			for (var i = 0; i < 250; i++)
			{
				Assert.Equal($"{i}", messages[249 - i].Content.Message);
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task History__Given_ConfiguredPubNub__When_Count250__Then_Fetch250InChronologicalOrder()
		{
			var expectedCount = 250;
			var expectedFirst = 14651593893212854;
			var expectedLast = 14651598898699987;

			var response = await Settings.Default.HistoryEncryptedHighVolumeChannel
				.EncryptedWith(Settings.Default.CipherKey)
				.ConfigurePubNub(c => c.SubscribeKey = Settings.Default.SubscribeKey)
				.History<HistoryTestMessage>(count: expectedCount, order: HistoryOrder.Chronological);

			Assert.NotNull(response.Messages);
			Assert.Equal(expectedCount, response.Messages.Length);

			Assert.Equal(expectedFirst, response.Oldest);
			Assert.Equal(expectedLast, response.Newest);

			var messages = response.Messages.ToArray();
			//assert messages are in proper order
			for (var i = 0; i > 250; i++)
			{
				Assert.Equal($"{i}", messages[i].Content.Message);
			}
		}

		public class HistoryTestMessage
		{
			[JsonProperty("message")]
			public string Message { get; set; }
		}

		public class ComplextHistoryTestMessage
		{
			[JsonProperty("header")]
			public string Header { get; set; }

			[JsonProperty("content")]
			public HistoryTestMessage Content { get; set; }
		}
	}
}