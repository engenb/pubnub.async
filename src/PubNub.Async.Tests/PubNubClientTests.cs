using Ploeh.AutoFixture;
using PubNub.Async.Models.Channel;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests
{
	public class PubNubClientTests : AbstractTest
	{
		[Fact]
		public void ctor__Then_CloneEnvironment()
		{
			PubNub.Configure(c =>
			{
				c.AuthenticationKey = Fixture.Create<string>();
				c.CipherKey = Fixture.Create<string>();
				c.MinutesToTimeout = Fixture.Create<int>();
				c.Origin = Fixture.Create<string>();
				c.PublishKey = Fixture.Create<string>();
				c.SecretKey = Fixture.Create<string>();
				c.SessionUuid = Fixture.Create<string>();
				c.SslEnabled = Fixture.Create<bool>();
				c.SubscribeKey = Fixture.Create<string>();
			});

			var subject = new PubNubClient(Fixture.Create<string>());

			Assert.NotSame(PubNub.Environment, subject.Environment);

			Assert.Equal(PubNub.Environment.AuthenticationKey, subject.Environment.AuthenticationKey);
			Assert.Equal(PubNub.Environment.CipherKey, subject.Environment.CipherKey);
			Assert.Equal(PubNub.Environment.MinutesToTimeout, subject.Environment.MinutesToTimeout);
			Assert.Equal(PubNub.Environment.Origin, subject.Environment.Origin);
			Assert.Equal(PubNub.Environment.PublishKey, subject.Environment.PublishKey);
			Assert.Equal(PubNub.Environment.SecretKey, subject.Environment.SecretKey);
			Assert.Equal(PubNub.Environment.SessionUuid, subject.Environment.SessionUuid);
			Assert.Equal(PubNub.Environment.SslEnabled, subject.Environment.SslEnabled);
			Assert.Equal(PubNub.Environment.SubscribeKey, subject.Environment.SubscribeKey);

			PubNub.Environment.Reset();
		}

		[Fact]
		public void Secured__Then_SecureChannel()
		{
			var channel = new Channel(Fixture.Create<string>());
			var subject = new PubNubClient(channel);

			subject.Secured();

			Assert.True(channel.Secured);
		}

		[Fact]
		public void SecuredWith__Given_AuthKey__Then_SecureChannelWithAuthKey()
		{
			var expectedAuthKey = Fixture.Create<string>();
			var channel = new Channel(Fixture.Create<string>());
			var subject = new PubNubClient(channel);

			subject.SecuredWith(expectedAuthKey);

			Assert.True(channel.Secured);
			Assert.Equal(expectedAuthKey, subject.Environment.AuthenticationKey);
		}

		[Fact]
		public void Encrypted__Then_EncryptChannel()
		{
			var channel = new Channel(Fixture.Create<string>());
			var subject = new PubNubClient(channel);

			subject.Encrypted();

			Assert.True(channel.Encrypted);
		}

		[Fact]
		public void EncryptedWith__Given_Cipher__Then_EncryptedChannelWithCipher()
		{
			var expectedCipher = Fixture.Create<string>();

			var channel = new Channel(Fixture.Create<string>());
			var subject = new PubNubClient(channel);

			subject.EncryptedWith(expectedCipher);

			Assert.True(channel.Encrypted);
			Assert.Equal(expectedCipher, channel.Cipher);
		}
	}
}