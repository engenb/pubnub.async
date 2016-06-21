using System;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Publish;
using PubNub.Async.Tests.Common.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.Publish
{
	public class PublishServiceTests : IDisposable
	{
		[Fact]
		public async Task Publish__Given_Message__When_ChannelEncryptedButNoCipher__Then_UseSettingsCipher()
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
				httpTest.RespondWithJson(200, new object[] { 1, expectedResultMessage, expectedResultSent });

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

		public void Dispose()
		{
			PubNub.Environment.Reset();
		}
	}
}
