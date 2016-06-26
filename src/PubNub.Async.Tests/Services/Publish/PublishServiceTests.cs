using System.Threading.Tasks;
using Flurl;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using PCLCrypto;
using Ploeh.AutoFixture;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Access;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Publish;
using PubNub.Async.Tests.Common;
using PubNub.Async.Tests.Common.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.Publish
{
	public class PublishServiceTests : AbstractTest
	{
		[Fact]
		public async Task Publish__Given_Message__When_ChannelEncryptedButNoCipher__Then_UseEnvironmentCipher()
		{
			var message = new PublishTestMessage {Message = "TEST"};
			var serializedMessage = JsonConvert.SerializeObject(message);

			var expectedCipher = "CIPHER";

			var expectedResultSuccess = true;
			var expectedResultMessage = "Sent";
			var expectedResultSent = 1234;

			var mockCrypto = new Mock<ICryptoService>();
			mockCrypto
				.Setup(x => x.Encrypt(null, serializedMessage))
				.Returns("ENCRYPTED");

			var mockAccess = new Mock<IAccessManager>();

			var client = "channel"
				.Encrypted()
				.ConfigurePubNub(c =>
				{
					c.PublishKey = Settings.Default.NoFeaturesPublishKey;
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
					c.CipherKey = expectedCipher;
				});

			var subject = new PublishService(client, mockCrypto.Object, mockAccess.Object);

			PublishResponse response;

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] {1, expectedResultMessage, expectedResultSent});

				response = await subject.Publish(message, false);

				//no need to assert anything here as per request sent (covered in another test)
			}

			Assert.NotNull(response);
			Assert.Equal(expectedResultSuccess, response.Success);
			Assert.Equal(expectedResultMessage, response.Message);
			Assert.Equal(expectedResultSent, response.Sent);

			mockCrypto.Verify(x => x.Encrypt(expectedCipher, It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task Publish__Given_Message__When_ChannelEncryptedWithCipher__Then_UseChannelCipher()
		{
			var message = new PublishTestMessage {Message = "TEST"};
			var serializedMessage = JsonConvert.SerializeObject(message);

			var expectedCipher = "CIPHER";

			var expectedResultSuccess = true;
			var expectedResultMessage = "Sent";
			var expectedResultSent = 1234;

			var mockCrypto = new Mock<ICryptoService>();
			mockCrypto
				.Setup(x => x.Encrypt(expectedCipher, serializedMessage))
				.Returns("ENCRYPTED");

			var mockAccess = new Mock<IAccessManager>();

			var client = "channel"
				.ConfigurePubNub(c =>
				{
					c.PublishKey = Settings.Default.NoFeaturesPublishKey;
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
					c.CipherKey = "OTHER_CIPHER";
				})
				.EncryptedWith(expectedCipher);

			var subject = new PublishService(client, mockCrypto.Object, mockAccess.Object);

			PublishResponse response;

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] {1, expectedResultMessage, expectedResultSent});

				response = await subject.Publish(message, false);

				//no need to assert anything here as per request sent (covered in another test)
			}

			Assert.NotNull(response);
			Assert.Equal(expectedResultSuccess, response.Success);
			Assert.Equal(expectedResultMessage, response.Message);
			Assert.Equal(expectedResultSent, response.Sent);

			mockCrypto.Verify(x => x.Encrypt(expectedCipher, It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task Publish__Given_Message__When_SecuredAndGrantFails__Then_ReturnErrorMessage()
		{
			var expectedGrantResponseMessage = Fixture.Create<string>();
			var expectedGrantResponse = new GrantResponse {Success = false, Message = expectedGrantResponseMessage};

			var message = new PublishTestMessage {Message = "TEST"};

			var client = "channel"
				.Secured();

			var mockAccess = new Mock<IAccessManager>();
			mockAccess
				.Setup(x => x.Establish(AccessType.Write))
				.ReturnsAsync(expectedGrantResponse);

			var subject = new PublishService(client, Mock.Of<ICryptoService>(), mockAccess.Object);

			var result = await subject.Publish(message, false);

			Assert.NotNull(result);
			Assert.False(result.Success);
			Assert.Equal(expectedGrantResponseMessage, result.Message);
			Assert.Equal(0, result.Sent);
		}

		[Fact]
		public async Task Publish__Given_Message__When_SecretKey__Then_AppendSignature()
		{
			var expectedSignature = Fixture.Create<string>();

			var expectedResponseMessage = Fixture.Create<string>();
			var expectedResponseSent = Fixture.Create<long>();

			var pubKey = Fixture.Create<string>();
			var subKey = Fixture.Create<string>();
			var secKey = Fixture.Create<string>();
			var channel = Fixture.Create<string>();
			var messageContent = Fixture.Create<string>();

			var message = new PublishTestMessage {Message = messageContent};
			var serializedMessage = JsonConvert.SerializeObject(message);

			var client = channel
				.ConfigurePubNub(c =>
				{
					c.PublishKey = pubKey;
					c.SubscribeKey = subKey;
					c.SecretKey = secKey;
				});

			var uri = pubKey.AppendPathSegments(subKey, secKey, channel, serializedMessage);

			var mockCrypto = new Mock<ICryptoService>();
			mockCrypto
				.Setup(x => x.Hash(uri, HashAlgorithm.Md5))
				.Returns(expectedSignature);

			var subject = new PublishService(client, mockCrypto.Object, Mock.Of<IAccessManager>());

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(new object[] {true, expectedResponseMessage, expectedResponseSent});

				var result = await subject.Publish(message, false);

				Assert.NotNull(result);
				Assert.True(result.Success);
				Assert.Equal(expectedResponseMessage, result.Message);
				Assert.Equal(expectedResponseSent, result.Sent);

				httpTest
					.ShouldHaveCalled($"https://pubsub.pubnub.com/publish/{pubKey}/{subKey}/{expectedSignature}/*");
			}
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Publish__Given_Message__When_NoSecretKeyAndNotEncrypted__Then_Publish()
		{
			var message = new PublishTestMessage
			{
				Message = "Hello World!"
			};

			var response = await Settings.Default.PublishDecryptedChannel
				.ConfigurePubNub(c =>
				{
					c.PublishKey = Settings.Default.NoFeaturesPublishKey;
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
				})
				.Publish(message, false);

			Assert.True(response.Success);
			Assert.Equal("Sent", response.Message);
			Assert.True(response.Sent > 0);
		}

		[Fact]
		[Trait("Category", "integration")]
		public async Task Publish__Given_Message__When_NoSecretKeyAndEncrypted__Then_Publish()
		{
			var message = new PublishTestMessage
			{
				Message = "Hello World!"
			};

			var response = await Settings.Default.PublishEncryptedChannel
				.EncryptedWith(Settings.Default.CipherKey)
				.ConfigurePubNub(c =>
				{
					c.PublishKey = Settings.Default.NoFeaturesPublishKey;
					c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
				})
				.Publish(message, false);

			Assert.True(response.Success);
			Assert.Equal("Sent", response.Message);
			Assert.True(response.Sent > 0);
		}

		public class PublishTestMessage
		{
			public string Message { get; set; }
		}
	}
}