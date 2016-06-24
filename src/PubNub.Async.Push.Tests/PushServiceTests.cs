using Flurl.Http.Testing;
using Moq;
using PubNub.Async;
using PubNub.Async.Extensions;
using PubNub.Async.Models.Publish;
using PubNub.Async.Services.Publish;
using PubNub.Async.Push.Models;
using PubNub.Async.Push.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PubNub.Async.Push.Tests
{
    public class PushServiceTests
    {
        private PushService CreateSubject(
            IPubNubClient client = null,
            IPublishService publish = null)
        {
            var mockClient = client ??
                "channel"
                    .ConfigurePubNub(c =>
                    {
                        c.SubscribeKey = "subkey";
                        c.SslEnabled = true;
                    });

            var mockPublish = publish ?? new Mock<IPublishService>().Object;

            return new PushService(mockClient, mockPublish);
        }

        [Fact]
        public async Task Register__Given_NoTokenProvided__Then_ThrowsException()
        {
            var subject = CreateSubject();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => subject.Register(DeviceType.Android, null));
            Assert.Equal("token", exception.ParamName);
        }

        [Fact]
        public async Task Register__Given_NoBodyReturnedInResponse__Then_ReturnsError()
        {
            var subject = CreateSubject();
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(200, null);
                var response = await subject.Register(DeviceType.Android, "token");
                Assert.False(response.Success);
                Assert.NotNull(response.Message);
            }
        }

        [Fact]
        public async Task Register__Given_BodyReturnedInResponse__Then_ReturnsEmptyResponse()
        {
            var expectedMessage = "Modified Channels";
            var subject = CreateSubject();
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(200, new object[] { 1, expectedMessage });
                var response = await subject.Register(DeviceType.Android, "token");
                Assert.True(response.Success);
                Assert.Equal(expectedMessage, response.Message);
            }
        }

        [Fact]
        public async Task Revoke__Given_NoTokenProvided__Then_ThrowsException()
        {
            var subject = CreateSubject();
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => subject.Revoke(DeviceType.Android, null));
            Assert.Equal("token", exception.ParamName);
        }

        [Fact]
        public async Task Revoke__Given_NoBodyReturnedInResponse__Then_ReturnsError()
        {
            var subject = CreateSubject();
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(200, null);
                var response = await subject.Revoke(DeviceType.Android, "token");
                Assert.False(response.Success);
                Assert.NotNull(response.Message);
            }
        }

        [Fact]
        public async Task Revoke__Given_BodyReturnedInResponse__Then_ReturnsEmptyResponse()
        {
            var expectedMessage = "Modified Channels";
            var subject = CreateSubject();
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(200, new object[] { 1, expectedMessage });
                var response = await subject.Revoke(DeviceType.Android, "token");
                Assert.True(response.Success);
                Assert.Equal(expectedMessage, response.Message);
            }
        }

        [Fact]
        public async Task Publish__Given_ChannelEncrypted__Then_ThrowsException()
        {
            var client = "channel"
                .ConfigurePubNub(c =>
                {
                    c.SubscribeKey = "subkey";
                    c.SslEnabled = true;
                })
                .Encrypted();

            var subject = CreateSubject(client);
            await Assert.ThrowsAsync<InvalidOperationException>(() => subject.PublishPushNotification("whatever"));
        }

        [Fact]
        public async Task Publish__Given_ValidChannel__Then_Publishes()
        {
            var message = "weeeeeeeeee";
            PushPayload payload = null;

            var mockPublish = new Mock<IPublishService>();
            mockPublish
                .Setup(mock => mock.Publish(It.IsAny<PushPayload>(), false))
                .Callback<PushPayload, bool>((p, h) => payload = p)
                .Returns(Task.FromResult(new PublishResponse()));

            var subject = CreateSubject(publish: mockPublish.Object);
            var response = await subject.PublishPushNotification(message);

            mockPublish.Verify(mock => mock.Publish(It.IsAny<PushPayload>(), false), Times.Once());
            Assert.Equal(message, payload.Apns.Aps.Alert);
            Assert.Equal(message, payload.Gcm.Data.Message);
        }
    }
}