using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Moq;
using Newtonsoft.Json;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Channel;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Access;
using PubNub.Async.Services.Crypto;
using PubNub.Async.Services.Publish;
using PubNub.Async.Tests.Properties;
using Xunit;

namespace PubNub.Async.Tests.Services.Publish
{
	public class PublishServiceTests : IDisposable
	{
		public PublishServiceTests()
		{
			PubNub.Configure(c =>
			{
				c.PublishKey = Settings.Default.NoFeaturesPublishKey;
				c.SubscribeKey = Settings.Default.NoFeaturesSubscribeKey;
			});
		}

		[Fact]
		public async Task Publish__Given_Message__When_ChannelEncryptedButNoCipher__Then_UseSettingsCipher()
		{
			var expectedCipher = "CIPHER";

			var expectedSuccess = true;
			var expectedMessage = "Sent";
			var expectedSent = 1234;
			
			var mockCrypto = new Mock<ICryptoService>();
			var mockAccess = new Mock<IAccessManager>();

			var client = "channel"
				.Encrypted()
				.ConfigurePubNub(c => c.CipherKey = expectedCipher);

			var subject = new PublishService(client, mockCrypto.Object, mockAccess.Object);

			PublishResponse response;

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] {1, expectedMessage, expectedSent});

				response = await subject.Publish(new PublishTestMessage { Message = "TEST" }, false);

				//no need to assert anything here as per request sent (covered in another test)
			}

			Assert.NotNull(response);
			Assert.Equal(expectedSuccess, response.Success);
			Assert.Equal(expectedMessage, response.Message);
			Assert.Equal(expectedSent, response.Sent);

			mockCrypto.Verify(x => x.Encrypt(expectedCipher, It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task Publish__Given_Message__When_ChannelEncryptedWithCipher__Then_UseSettingsCipher()
		{
			var expectedCipher = "CIPHER";

			var expectedSuccess = true;
			var expectedMessage = "Sent";
			var expectedSent = 1234;
			
			var mockCrypto = new Mock<ICryptoService>();
			var mockAccess = new Mock<IAccessManager>();

			var client = "channel"
				.EncryptedWith(expectedCipher)
				.ConfigurePubNub(c => c.CipherKey = "OTHER_CIPHER");

			var subject = new PublishService(client, mockCrypto.Object, mockAccess.Object);

			PublishResponse response;

			using (var httpTest = new HttpTest())
			{
				httpTest.RespondWithJson(200, new object[] { 1, expectedMessage, expectedSent });

				response = await subject.Publish(new PublishTestMessage { Message = "TEST" }, false);

				//no need to assert anything here as per request sent (covered in another test)
			}

			Assert.NotNull(response);
			Assert.Equal(expectedSuccess, response.Success);
			Assert.Equal(expectedMessage, response.Message);
			Assert.Equal(expectedSent, response.Sent);

			mockCrypto.Verify(x => x.Encrypt(expectedCipher, It.IsAny<string>()), Times.Once);
		}

		[Fact(Skip = "L&L")]
		[Trait("Category", "integration")]
		public async Task Publish__Given_Message__When_NoSecretKeyAndNotEncrypted__Then_Publish()
		{
			var message = new PublishTestMessage
			{
				Message = "Hello World!"
			};

			var response = await Settings.Default.PublishDecryptedChannel
				.Publish(message, false);

			Assert.True(response.Success);
			Assert.Equal("Sent", response.Message);
			Assert.True(response.Sent > 0);
		}

		[Fact(Skip = "L&L")]
		[Trait("Category", "integration")]
		public async Task Publish__Given_Message__When_NoSecretKeyAndEncrypted__Then_Publish()
		{
			var message = new PublishTestMessage
			{
				Message = "Hello World!"
			};

			var response = await Settings.Default.PublishEncryptedChannel
				.EncryptedWith(Settings.Default.CipherKey)
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
			PubNub.GlobalSettings.Reset();
		}
	}
}
