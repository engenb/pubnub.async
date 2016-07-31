using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ploeh.AutoFixture;
using PubNub.Async.Extensions;
using PubNub.Async.Models;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Extensions
{
	public class ChannelExtensionsTests : AbstractTest
	{
		[Fact]
		public void Encrypted__Given_String__Then_ConfigureClient()
		{
			var channelName = Fixture.Create<string>();

			var result = channelName.Encrypted();

			Assert.NotNull(result);
			Assert.Equal(channelName, result.Channel.Name);
			Assert.True(result.Channel.Encrypted);
		}

		[Fact]
		public void Encrypted__Given_Channel__Then_ConfigureClient()
		{
			var channelName = Fixture.Create<string>();
			var channel = new Channel(channelName);

			var result = channel.Encrypted();

			Assert.NotNull(result);
			Assert.Equal(channelName, result.Channel.Name);
			Assert.True(result.Channel.Encrypted);
		}

		[Fact]
		public void EncryptedWith__Given_StringAndCipher__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedCipher = Fixture.Create<string>();

			var result = expectedName.EncryptedWith(expectedCipher);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Encrypted);
			Assert.Equal(expectedCipher, result.Channel.Cipher);
		}

		[Fact]
		public void EncryptedWith__Given_ChannelAndCipher__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedCipher = Fixture.Create<string>();

			var channel = new Channel(expectedName);

			var result = channel.EncryptedWith(expectedCipher);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Encrypted);
			Assert.Equal(expectedCipher, result.Channel.Cipher);
		}

		[Fact]
		public void Secured__Given_String__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();

			var result = expectedName.Secured();

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Null(result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void Secured__Given_String__When_MinsToTimeout__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedMinsToTimeout = Fixture.Create<int>();

			var result = expectedName.Secured(expectedMinsToTimeout);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedMinsToTimeout, result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void Secured__Given_ChannelAndMinsToTimeout__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedMinsToTimeout = Fixture.Create<int>();

			var channel = new Channel(expectedName);

			var result = channel.Secured(expectedMinsToTimeout);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedMinsToTimeout, result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void Secured__Given_Channel__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var channel = new Channel(expectedName);

			var result = channel.Secured();

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Null(result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void SecuredWith__Given_StringAndAuthKey__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedAuthKey = Fixture.Create<string>();

			var result = expectedName.SecuredWith(expectedAuthKey);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedAuthKey, result.Environment.AuthenticationKey);
			Assert.Null(result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void SecuredWith__Given_StringAndAuthKeyAndMinsToTimeout__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedAuthKey = Fixture.Create<string>();
			var expectedMinsToTimeout = Fixture.Create<int>();

			var result = expectedName.SecuredWith(expectedAuthKey, expectedMinsToTimeout);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedAuthKey, result.Environment.AuthenticationKey);
			Assert.Equal(expectedMinsToTimeout, result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void SecuredWith__Given_ChannelAndAuthKey__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedAuthKey = Fixture.Create<string>();
			var channel = new Channel(expectedName);

			var result = channel.SecuredWith(expectedAuthKey);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedAuthKey, result.Environment.AuthenticationKey);
			Assert.Null(result.Environment.MinutesToTimeout);
		}

		[Fact]
		public void SecuredWith__Given_ChannelAuthKeyAndMinsToTimeout__Then_ConfigureClient()
		{
			var expectedName = Fixture.Create<string>();
			var expectedAuthKey = Fixture.Create<string>();
			var expectedMinsToTimeout = Fixture.Create<int>();
			var channel = new Channel(expectedName);

			var result = channel.SecuredWith(expectedAuthKey, expectedMinsToTimeout);

			Assert.NotNull(result);
			Assert.Equal(expectedName, result.Channel.Name);
			Assert.True(result.Channel.Secured);
			Assert.Equal(expectedAuthKey, result.Environment.AuthenticationKey);
			Assert.Equal(expectedMinsToTimeout, result.Environment.MinutesToTimeout);
		}
	}
}
