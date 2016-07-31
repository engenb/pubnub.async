using System;
using Ploeh.AutoFixture;
using PubNub.Async.Models;
using PubNub.Async.Tests.Common;
using Xunit;

namespace PubNub.Async.Tests.Models
{
	public class ChannelTests : AbstractTest
	{
		[Fact]
		public void ctor__Given_ChannelName__Then_SetName()
		{
			var expectedChannelName = Fixture.Create<string>();
			var subject = new Channel(expectedChannelName);

			Assert.Equal(expectedChannelName, subject.Name);
		}

		[Fact]
		public void ctor__Given_NullChannelName__Then_ThrowEx()
		{
			var ex = Assert.Throws<ArgumentException>(() => new Channel(null));
			Assert.Equal(ex.ParamName, "name");
		}

		[Fact]
		public void ctor__Given_WhitespaceChannelName__Then_ThrowEx()
		{
			var ex = Assert.Throws<ArgumentException>(() => new Channel(" "));
			Assert.Equal(ex.ParamName, "name");
		}

		[Fact]
		public void Clone__Given_Channel__Then_ReturnClonedChannel()
		{
			var expectedChannelName = Fixture.Create<string>();

			var subject = Fixture.Create(new Channel(expectedChannelName));

			var result = subject.Clone();

			Assert.NotSame(subject, result);
			Assert.Equal(subject.Name, result.Name);
			Assert.Equal(subject.Cipher, result.Cipher);
			Assert.Equal(subject.Encrypted, result.Encrypted);
			Assert.Equal(subject.Secured, result.Secured);
		}

		[Fact]
		public void GetHashCode__Given_Channel__Then_ReturnNameHash()
		{
			var channel = Fixture.Create<string>();
			var subject = new Channel(channel);

			var result = subject.GetHashCode();

			Assert.Equal(channel.GetHashCode(), result);
		}

		[Fact]
		public void Equals__Given_Object__When_ChannelNamesEqual__Then_ReturnTrue()
		{
			var name = Fixture.Create<string>();
			var subjectA = new Channel(name);
			var subjectB = new Channel(name);

			Assert.NotSame(subjectA, subjectB);
			Assert.Equal(subjectA, subjectB);
		}

		[Fact]
		public void Equals__Given_Object__When_NotChannel__Then_ReturnFalse()
		{
			var name = Fixture.Create<string>();
			var subject = new Channel(name);

			Assert.NotEqual(subject, new object());
		}

		[Fact]
		public void Equals__Given_Channel__When_Null__Then_ReturnFalse()
		{
			var name = Fixture.Create<string>();
			var subject = new Channel(name);

			Assert.NotEqual(subject, null);
		}
	}
}